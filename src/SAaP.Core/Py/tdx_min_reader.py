# -*- coding: UTF-8 -*-

import logging
import os
import sys
import struct
import math
import datetime

# args
# args[1] tdx path
# args[2] out path
# args[3] 1/5 1min line or 5min line
# args[4] specific code or all

if len(sys.argv) < 4:
    print('no enough args!')
    sys.exit()
elif len(sys.argv) == 4:
    tdx_path = sys.argv[1]
    print('tdx path: ' + tdx_path)
    out_path = sys.argv[2]
    print('out path: ' + out_path)
    line_type = sys.argv[3]
    print('line type: ' + line_type + 'min line')
elif len(sys.argv) == 5:
    tdx_path = sys.argv[1]
    print('tdx path: ' + tdx_path)
    out_path = sys.argv[2]
    print('out path: ' + out_path)
    line_type = sys.argv[3]
    print('line type: ' + line_type + 'min line')
    quest_code = sys.argv[4]
    print('query codes: ' + quest_code)
else:
    print('args too much')
    sys.exit()

logging.basicConfig(filename=os.path.join(out_path, 'py_log.txt'), level=logging.INFO,
                    format='%(asctime)s %(levelname)s: %(message)s', datefmt='%m/%d/%Y %I:%M:%S %p')

logging.info("start => tdx_reader.py")
logging.info("tdx_path =>" + tdx_path)
logging.info("out_path =>" + out_path)
logging.info("line_type =>" + line_type + 'min')

# 可转债的值变换与普通股有出入


def zz_parse(zzc, original):

    # print(zzc)
    if zzc == "11" or zzc == "12":
        return int(str(original)[0:6]) / 1000
    else:
        return original / 100


def add_zero(x):
    if x >= 10:
        return '' + str(x)
    else:
        return '0' + str(x)

def store_stock_data_to_csv(filepath, name):

    logging.info(": exec store_stock_data_to_csv => " + name)
    logging.info(": exec path => " + filepath)

    # 代码前2位
    zzc = name[2:4]

    with open(filepath, 'rb') as f:
        file_object_path = out_path + '/' + name + '.'+ line_type +'min.csv'
        file_object = open(file_object_path, 'w+')
        while True:
            stock_date = f.read(2)
            stock_time = f.read(2)
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
            stock_date = struct.unpack("h", stock_date)       # 2字节 20
            stock_time = struct.unpack("h", stock_time)       # 2字节 12
            stock_open = struct.unpack("f", stock_open)       # 开盘价*100
            stock_high = struct.unpack("f", stock_high)       # 最高价*100
            stock_low = struct.unpack("f", stock_low)         # 最低价*100
            stock_close = struct.unpack("f", stock_close)     # 收盘价*100
            stock_amount = struct.unpack("f", stock_amount)   # 成交额
            stock_vol = struct.unpack("i", stock_vol)         # 成交量
            stock_reserv = struct.unpack("i", stock_reserv)   # 保留值

            year = math.floor(stock_date[0] / 2048) + 2036
            month =int((stock_date[0] % 2048) / 100)
            day = (stock_date[0] % 2048) % 100
            hour = int(stock_time[0] / 60)
            min = stock_time[0] - int(stock_time[0] / 60) * 60

            full_dt = datetime.datetime(year, month, day, hour, min)

            list = full_dt.strftime('%Y/%m/%d') + "," + full_dt.strftime('%H:%M') + ',' + '{:.3f}'.format(stock_open[0]) + "," + '{:.3f}'.format(stock_high[0]) + "," + '{:.3f}'.format(
            stock_low[0]) + "," + '{:.3f}'.format(stock_close[0]) + "," + str(stock_vol[0]) + "\n"

            file_object.writelines(list)
        file_object.close()


if line_type == '1':
    ex_dir = 'minline'
elif line_type == '5':
    ex_dir = 'fzline'
else:
    print('min type not support')
    sys.exit()


data_sh = tdx_path + '/vipdoc/sh/' + ex_dir + '/'
data_sz = tdx_path + '/vipdoc/sz/' + ex_dir + '/'

if 'quest_code' in globals():

    logging.info("quest code =>" + quest_code)

    # exec specific code
    for i in quest_code.split(','):
        if len(i) == 6:
            code_name = 'sh' + i + '.lc' + line_type
            if os.path.exists(data_sh + code_name):
                store_stock_data_to_csv(data_sh + code_name, 'sh' + i)
                continue

            code_name = 'sz' + i + '.lc' + line_type
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
                logging.info("neither shanghai or shenzhen =>" + i)
                print('neither shanghai or shenzhen')
                continue
            f_name = hd + code_name + '.day'
            f_path = tdx_path + '/vipdoc/' + hd + '/lday/' + f_name
            if os.path.exists(f_path):
                store_stock_data_to_csv(f_path, hd + code_name)
                continue
        else:
            logging.info("neither shanghai or shenzhen =>" + i)
            print('neither shanghai or shenzhen')
            continue
else:
    logging.info("exec all codes")
    # exec all code
    file_sh = os.listdir(data_sh)
    for i in file_sh:
        store_stock_data_to_csv(data_sh + i, i[:-4])

    file_sz = os.listdir(data_sz)
    for i in file_sz:
        store_stock_data_to_csv(data_sz + i, i[:-4])

logging.info("end .......")
print("finished")
