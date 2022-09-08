# -*- coding: UTF-8 -*-

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

# 可转债的值变换与普通股有出入
def zz_parse(zzc, original):

    # print(zzc)
    if zzc == "11" or zzc == "12":
        return int(str(original)[0:6]) / 1000
    else:
        return original / 100


def store_stock_data_to_csv(filepath, name):

    # 代码前2位
    zzc = name[2:4]

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
            stock_date = struct.unpack("l", stock_date)       # 4字节 如20091229
            stock_open = struct.unpack("l", stock_open)       # 开盘价*100
            stock_high = struct.unpack("l", stock_high)       # 最高价*100
            stock_low = struct.unpack("l", stock_low)         # 最低价*100
            stock_close = struct.unpack("l", stock_close)     # 收盘价*100
            stock_amount = struct.unpack("f", stock_amount)   # 成交额
            stock_vol = struct.unpack("l", stock_vol)         # 成交量
            stock_reserv = struct.unpack("l", stock_reserv)   # 保留值

            date_format = datetime.datetime.strptime(
                str(stock_date[0]), '%Y%M%d')  # 格式化日期

            list = date_format.strftime('%Y/%M/%d') + "," + str(zz_parse(zzc, stock_open[0])) + "," + str(zz_parse(zzc, stock_high[0])) + "," + str(
                zz_parse(zzc, stock_low[0])) + "," + str(zz_parse(zzc, stock_close[0])) + "," + str(stock_vol[0]) + "\n"

            file_object.writelines(list)
        file_object.close()


data_sh = tdx_path + '/vipdoc/sh/lday/'
data_sz = tdx_path + '/vipdoc/sz/lday/'

if 'quest_code' in globals():
    # exec specific code
    for i in quest_code.split(','):
        if len(i) != 6:
            break

        code_name = 'sh' + i + '.day'
        if os.path.exists(data_sh + code_name):
            store_stock_data_to_csv(data_sh + code_name, 'sh' + i)
            continue

        code_name = 'sz' + i + '.day'
        if os.path.exists(data_sz + code_name):
            store_stock_data_to_csv(data_sz + code_name, 'sz' + i)
            continue

        print('neither shanghai or shenzhen')
        sys.exit()
else:
    # exec all code
    file_sh = os.listdir(data_sh)
    for i in file_sh:
        store_stock_data_to_csv(data_sh + i, i[:-4])

    file_sz = os.listdir(data_sz)
    for i in file_sz:
        store_stock_data_to_csv(data_sz + i, i[:-4])

print("finished")