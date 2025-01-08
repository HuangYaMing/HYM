import os
import sys

# �����������Ϊ UTF-8
sys.stdout = open(sys.stdout.fileno(), mode='w', encoding='utf-8', buffering=1)

lines = [
    "This is the first line.\n",
    "This is the second line.\n",
    "This is the third line.\n"
]

# ��ȡ��ǰ����Ŀ¼
current_directory = os.getcwd()
print(current_directory)

# �ڵ�ǰ����Ŀ¼�´����ļ�
file_path = os.path.join(current_directory, 'example.txt')

# �����ļ��У�����ļ��в����ڣ�
os.makedirs('Test', exist_ok=True)

with open('Test/example.txt', 'w', encoding='utf-8') as file:
    file.writelines(lines)

with open(file_path, 'w', encoding='utf-8') as file1:
    file1.writelines(lines)

print(" 'example.txt'")