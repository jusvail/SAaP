import urllib.request
import time
import re

def get_min_price(code):
    """

    :param code:  输入股票代码
    :return: 输出实时价格：当前最新price
            最高价格：price_max
            最低价格：price_min
            今日开盘价： price_opening
    """
    url = 'http://qt.gtimg.cn/q=' + code.replace('.', '')
    response = urllib.request.urlopen(url)

    content = response.read().decode('GBK')
    print(content)

    row = re.findall(r"\d+\.\d\d", content)  # 正则匹配
    print(row)
    price = float(row[0])
    price_opening = float(row[2])
    price_max = float(row[20])
    price_min = float(row[21])
    print(price, price_max, price_min, price_opening)
    return price_opening, price_max, price_min, price
i = 0
while i <= 2:
    get_min_price('sz.002105')
    time.sleep(3)
    i = i + 1