# -*- coding: UTF-8 -*-

import logging
import os
import sys
import struct
import datetime


if len(sys.argv) < 3:
	print('no enough args!')
	sys.exit()
elif len(sys.argv) == 3:
	tdx_path = sys.argv[1]
	print('tdx path: ' + tdx_path)
	out_path = sys.argv[2]
	print('out path: ' + out_path)
elif len(sys.argv) == 4:
	tdx_path = sys.argv[1]
	print('tdx path: ' + tdx_path)
	out_path = sys.argv[2]
	print('out path: ' + out_path)
	quest_code = sys.argv[3]
	print('query codes: ' + quest_code)
else:
	print('args too much')
	sys.exit()

logging.basicConfig(filename=os.path.join(out_path, 'py_log.txt'), level=logging.INFO,
					format='%(asctime)s %(levelname)s: %(message)s', datefmt='%m/%d/%Y %I:%M:%S %p')

logging.info('start => tdx_reader.py')
logging.info('tdx_path =>' + tdx_path)
logging.info('out_path =>' + out_path)

# 可转债的值变换与普通股有出入


def zz_parse(zzc, original):

	# print(zzc)
	if zzc == '11' or zzc == '12':
		return int(str(original)[0:6]) / 1000
	elif zzc == 'us':
		return str(original)[0:7]
	else:
		return original / 100


def store_stock_data_to_csv(filepath, name):

	# 代码前2位
	zzc = name[2:4]
	if not ( zzc == '00' or zzc == '60' or zzc == '30' or zzc == '68' or zzc == '11' or zzc == '12'):
		return

	zzcN = name[2:5]
	if  zzcN == '114' or zzcN == '115' :
		return

	logging.info(': exec store_stock_data_to_csv => ' + name)
	logging.info(': exec path => ' + filepath)

	with open(filepath, 'rb') as f:
		file_object_path = out_path + '/' + name + '.csv'
		file_object = open(file_object_path, 'w+')
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
			stock_date = struct.unpack('l', stock_date)	   # 4字节 如20091229
			stock_open = struct.unpack('l', stock_open)	   # 开盘价*100
			stock_high = struct.unpack('l', stock_high)	   # 最高价*100
			stock_low = struct.unpack('l', stock_low)		 # 最低价*100
			stock_close = struct.unpack('l', stock_close)	 # 收盘价*100
			stock_amount = struct.unpack('f', stock_amount)   # 成交额
			stock_vol = struct.unpack('l', stock_vol)		 # 成交量
			stock_reserv = struct.unpack('l', stock_reserv)   # 保留值

			date_format = datetime.datetime.strptime(
				str(stock_date[0]), '%Y%M%d')  # 格式化日期

			list = date_format.strftime('%Y/%M/%d') + ',' + str(zz_parse(zzc, stock_open[0])) + ',' + str(zz_parse(zzc, stock_high[0])) + ',' + str(
				zz_parse(zzc, stock_low[0])) + ',' + str(zz_parse(zzc, stock_close[0])) + ',' + str(stock_vol[0]) + ',' + str(stock_amount[0]) + '\n'

			file_object.writelines(list)
		file_object.close()

def store_us_stock_data_to_csv(filepath, name):

	logging.info(': exec store_us_stock_data_to_csv => ' + name)
	logging.info(': exec path => ' + filepath)

	with open(filepath, 'rb') as f:
		file_object_path = out_path + '/' + name + '.csv'
		file_object = open(file_object_path, 'w+')
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
			stock_date = struct.unpack('l', stock_date)	   # 4字节 如20091229
			stock_open = struct.unpack('f', stock_open)	   # 开盘价*100
			stock_high = struct.unpack('f', stock_high)	   # 最高价*100
			stock_low = struct.unpack('f', stock_low)		 # 最低价*100
			stock_close = struct.unpack('f', stock_close)	 # 收盘价*100
			stock_amount = struct.unpack('f', stock_amount)   # 成交额
			stock_vol = struct.unpack('l', stock_vol)		 # 成交量
			stock_reserv = struct.unpack('l', stock_reserv)   # 保留值

			date_format = datetime.datetime.strptime(
				str(stock_date[0]), '%Y%M%d')  # 格式化日期

			list = date_format.strftime('%Y/%M/%d') + ',' + str(zz_parse('us', stock_open[0])) + ',' + str(zz_parse('us', stock_high[0])) + ',' + str(zz_parse('us', stock_low[0])) + ',' + str(zz_parse('us', stock_close[0])) + ',' + str(stock_vol[0]) + ',' + str(stock_amount[0]) + '\n'

			file_object.writelines(list)
		file_object.close()

data_sh = tdx_path + '/vipdoc/sh/lday/'
data_sz = tdx_path + '/vipdoc/sz/lday/'
data_us = tdx_path + '/vipdoc/ds/lday/'

if 'quest_code' in globals():

	logging.info('quest code =>' + quest_code)

	# exec specific code
	for i in quest_code.split(','):
		if len(i) == 6:
			code_name = 'sh' + i + '.day'
			if os.path.exists(data_sh + code_name):
				store_stock_data_to_csv(data_sh + code_name, 'sh' + i)
				continue

			code_name = 'sz' + i + '.day'
			if os.path.exists(data_sz + code_name):
				store_stock_data_to_csv(data_sz + code_name, 'sz' + i)
				continue

		elif len(i) == 7:
			flg = i[0:1]
			code_name = i[1:7]
			if flg == '0':
				hd = 'sz'
			elif flg == '1':
				hd = 'sh'
			else:
				logging.info('neither shanghai or shenzhen =>' + i)
				print('neither shanghai or shenzhen')
				continue
			f_name = hd + code_name + '.day'
			f_path = tdx_path + '/vipdoc/' + hd + '/lday/' + f_name
			if os.path.exists(f_path):
				store_stock_data_to_csv(f_path, hd + code_name)
				continue
		else:
			code_name = '74#' + i + '.day'
			if os.path.exists(data_us + code_name):
				store_us_stock_data_to_csv(data_us + code_name, 'us' + i)
				continue
else:
	logging.info('exec all codes')
	# exec all code
	file_sh = os.listdir(data_sh)
	for i in file_sh:
		store_stock_data_to_csv(data_sh + i, i[:-4])

	file_sz = os.listdir(data_sz)
	for i in file_sz:
		store_stock_data_to_csv(data_sz + i, i[:-4])
		
	file_us = os.listdir(data_us)
	for i in file_sz:
		store_us_stock_data_to_csv(data_us + i, i[:-4])

logging.info('end .......')
print('finished')
