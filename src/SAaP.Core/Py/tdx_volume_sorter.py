# -*- coding: UTF-8 -*-
import datetime
import logging
import os
import sys
import struct
import urllib.request
import time


class Summary:
	codeName = ''
	companyName = ''
	swap = 0.0
	mc = 0.0
	mcInCirculation = 0.0
	xDistance = 0


class TdxData:
	stock_date = 0
	stock_open = 0
	stock_high = 0
	stock_low = 0
	stock_close = 0
	stock_amount = 0
	stock_vol = 0
	stock_reserv = 0


class InArgs:
	tdx_path = ''
	out_path = ''
	closing_date = ''  # preserve


def pre_config():
	in_args = InArgs()
	if len(sys.argv) < 3:
		print('no enough args!')
		sys.exit()
	elif len(sys.argv) == 3:
		in_args.tdx_path = sys.argv[1]
		in_args.out_path = sys.argv[2]
	elif len(sys.argv) == 4:
		in_args.tdx_path = sys.argv[1]
		in_args.out_path = sys.argv[2]
		in_args.closing_date = sys.argv[3]
	else:
		print('args too much')
		sys.exit()

	print('tdx path: ' + in_args.tdx_path)
	print('out path: ' + in_args.out_path)
	print('closing date: ' + in_args.closing_date)

	# logging config
	logging.basicConfig(
		filename=os.path.join(in_args.out_path, 'py_log.txt'),
		level=logging.INFO,
		format='%(asctime)s %(levelname)s: %(message)s',
		datefmt='%m/%d/%Y %I:%M:%S %p',
	)
	logging.info('start => tdx_volume_sorter.py')
	logging.info('tdx_path =>' + in_args.tdx_path)
	logging.info('closing_date =>' + in_args.closing_date)

	return in_args


def get_stock_detail(codename):
	'''
	param:
		-> codename: 股票代码
		->   return: Summary
				-> 01: 名字
				-> 38: 换手率
				-> 44: 流通市值
				-> 45: 总市值
	'''
	url = 'https://qt.gtimg.cn/q=' + codename

	try:
		response = urllib.request.urlopen(url)
		content = response.read().decode('GBK')
	except:
		return

	summary = Summary()

	row = content.split('~')

	if len(row) < 1:
		return

	try:
		summary.codeName = codename
		summary.companyName = str(row[1])
		summary.swap = float(row[38])
		summary.mc = float(row[44]) * 100000000
		summary.mcInCirculation = float(row[45]) * 100000000
	except:
		return
	finally:
		return summary


# 函数，输入-传来的.day文件路径
# 1) 解析内容，并获取最近一天的市值以及流通值。
# 2) 从最后一天向前解析，根据获取到的(一般为当日的)市值，向前回溯
# 	从当日开始累加每日的成交量，直到累加的成交量等于当前市值。
# 	-> 意义：说明上涨到当前市值，经历了多少交易日的积累
# 	-> 注意index可能越界
# 3) 根据累积的天数x进行排序，一般的，如果x非常小，一般说明最近爆巨量，可能是短/中期见顶的信号
# 	如果x太长，说明很长一段时间以来该股票无人问津，也是不太好的情况
# 	理想情况是x~~30/60/90d => 表明买点可能就在附近
# 4) 将数据保存到list，待所有股票解析完毕后，根据x排序并输出到csv文件
def volume_sorter(fullpath, codename, closing_date):
	# indicate zz
	zzc = codename[2:4]
	# 股票以外的市值无法估计
	if not ( zzc == '00' or zzc == '60' or zzc == '30' or zzc == '68'):
		return
	# get summary per codename
	summary = get_stock_detail(codename)
	# codename may not to be real so return
	if summary is None:
		return
	# store all in day information
	tdx_all = []
	# loop file
	with open(fullpath, 'rb') as f:
		while True:
			stock_date = f.read(4)
			stock_open = f.read(4)
			stock_high = f.read(4)
			stock_low = f.read(4)
			stock_close = f.read(4)
			stock_amount = f.read(4)
			stock_vol = f.read(4)
			stock_reserv = f.read(4)

			# date,open,high,low,close,amount,vol,reservation

			if not stock_date:
				break

			tdx = TdxData()

			# 4字节 如20091229
			tdx.stock_date = datetime.datetime.strptime(str(struct.unpack('l', stock_date)[0]), '%Y%M%d')
			tdx.stock_date = datetime.datetime.strftime(tdx.stock_date, '%Y/%M/%d')
			# 指定某日的情况，后续的日期都不需要加到list
			if tdx.stock_date > closing_date:
				break
			tdx.stock_open =  struct.unpack('l', stock_open)[0]/100  # 开盘价*100
			tdx.stock_high =  struct.unpack('l', stock_high)[0]/100  # 最高价*100
			tdx.stock_low =  struct.unpack('l', stock_low)[0]/100  # 最低价*100
			tdx.stock_close =  struct.unpack('l', stock_close)[0]/100  # 收盘价*100
			tdx.stock_amount = struct.unpack('f', stock_amount)[0]  # 成交额
			tdx.stock_vol = struct.unpack('l', stock_vol)[0]  # 成交量
			tdx.stock_reserv = struct.unpack('l', stock_reserv)[0]  # 保留值
			tdx_all.append(tdx)
	# list count
	index_last = len(tdx_all) - 1
	# times
	times = 1
	maxTimes = 1
	# tmp mc value
	tmp_mc = 0.0
	tmp_mc_expect = 0.0
	# result
	found = -1
	# 100E
	e1 = 10000000000
	if summary.mc < e1:
		maxTimes = 5
	elif summary.mc < e1*3:
		maxTimes = 2
	elif summary.mc < e1*10:
		maxTimes = 1
	elif  summary.mc > e1*100: #1wE 以上，忽略
		return
	# loop list
	for i in range(index_last, -1, -1):
		# 向前循环到最多5倍倍当前市值，并且涨幅应该足够大
		tmp_mc_expect = times * summary.mc
		if times <= 5:
			# 最好大于100%
			tmp_mc += tdx_all[i].stock_amount
			# 达到N倍市值
			if tmp_mc > tmp_mc_expect:
				op = tdx_all[i].stock_open
				cl = tdx_all[index_last].stock_close
				# X日前到现在的涨跌
				zd = 100 * (cl - op) / op
				# 最少已近上涨了80% -> 很宽松了已经
				if zd < 80:
					# increase times
					times += 1
				else:
					found = index_last - i + 1
					break
		else:
			break
	if found < 0:
		return
	summary.xDistance = found
	return summary


def loop_file_per_dir(arg):
	file_name = time.strftime('%y-%m-%d', time.localtime()) + '.csv'
	out_file = os.path.join(arg.out_path,file_name)

	file_obj = open(out_file,'w+',encoding='utf-8')
	print('Loading',end='')
	for path in arg.tdx_path:
		files = os.listdir(path)
		for file_name in files:
			summary = volume_sorter(path + file_name, file_name[:-4], arg.closing_date)
			if summary is None:
				continue
			line = summary.codeName+','+summary.companyName+','+str(summary.xDistance)+','+str(summary.mc)+','+str(summary.mcInCirculation)
			print('.',end='',flush=True)
			print(line, file=file_obj, flush=True)
			#time.sleep(0.2)
	file_obj.close()


def main(args):

	tdx_dir = args.tdx_path
	args.tdx_path = [tdx_dir+'/vipdoc/sh/lday/',tdx_dir+'/vipdoc/sz/lday/']

	loop_file_per_dir(args)
	return


# summary = get_stock_detail('sz002992')
# print(summary.companyName)
if __name__ == '__main__':
	args = pre_config()
	main(args)
