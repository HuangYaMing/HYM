import os
import sys

# 设置输出编码为 UTF-8
sys.stdout = open(sys.stdout.fileno(), mode='w', encoding='utf-8', buffering=1)

lines = [
    "This is the first line.\n",
    "This is the second line.\n",
    "This is the third line.\n"
]

# 获取当前工作目录
current_directory = os.getcwd()
print(current_directory)

# 在当前工作目录下创建文件
file_path = os.path.join(current_directory, 'example.txt')

# 创建文件夹（如果文件夹不存在）
os.makedirs('Test', exist_ok=True)

with open('Test/example.txt', 'w', encoding='utf-8') as file:
    file.writelines(lines)

with open(file_path, 'w', encoding='utf-8') as file1:
    file1.writelines(lines)

print(" 'example.txt'")