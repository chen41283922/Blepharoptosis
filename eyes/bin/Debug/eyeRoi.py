import os
import sys
import dlib
import cv2
import glob

detector = dlib.simple_object_detector("detector.svm")
img = cv2.imread(sys.argv[1])
dets = detector(img)
for index, face in enumerate(dets):
    left = face.left()
    top = face.top()
    right = face.right()
    bottom = face.bottom()

print(left)
print(top)
print(right)
print(bottom)
