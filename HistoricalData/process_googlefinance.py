import argparse
import sys, os

parser = argparse.ArgumentParser(description='Process Google Finance data')
parser.add_argument('file', type=str)

args = parser.parse_args()
lines = open(args.file).readlines()
close(args.file)

interval_seconds = int(get_value(lines[3]))
headers = get_value(lines[4])

def get_value(line):
    return line.split('=')[1]