from random import shuffle
import glob
import numpy as np
import tensorflow as tf
import cv2
import sys

# sys Args:
# 1: img width
# 2: img height
# 3: img type (1=RGB | 2=GrayScale)
# 4: final fileName

labels = np.loadtxt("labels.txt", dtype='str')

def load_image(addr):      
    # read an image and resize to (224, 224)
    # cv2 load images as BGR, convert it to RGB
    img = cv2.imread(addr)
    img = cv2.resize(img, (int(sys.argv[1]), int(sys.argv[2])), interpolation=cv2.INTER_CUBIC)
    if int(sys.argv[3]) == 1:
        img = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
    elif int(sys.argv[3]) == 2:
        img = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
    img = img.astype(np.float32)
    return img

addrs = []
types = ["*.jpg", "*.png"]
for files in types:
	addrs.extend(glob.glob(files))

def _int64_feature(value):
    return tf.train.Feature(int64_list=tf.train.Int64List(value=[value]))
def _bytes_feature(value):
    return tf.train.Feature(bytes_list=tf.train.BytesList(value=[value]))


train_filename = sys.argv[4] + '.tfrecords'
writer = tf.python_io.TFRecordWriter(train_filename)

for i in range(len(addrs)):
	img = load_image(addrs[i])

	for lab in labels:
		if addrs[i].find(lab.partition(":")[0]):
			label = int (lab.partition(":")[2])
			break

	feature = {'label': _int64_feature(label), 
			   'image': _bytes_feature(tf.compat.as_bytes(img.tostring()))}

	example = tf.train.Example(features=tf.train.Features(feature=feature))
	writer.write(example.SerializeToString())

writer.close()
sys.stdout.flush()
























