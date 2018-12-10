import cv2
import dlib
import sys
detector = dlib.get_frontal_face_detector()
#landmark_predictor = dlib.shape_predictor("E:\\google download\\shape_predictor_68_face_landmarks.dat")
#img = cv2.imread('E:\\Data\\withlight\\2018912171741.bmp')
#img = cv2.imread('E:\\Data\\Hospital\\Hospital\\fail\\10535045.jpg')
landmark_predictor = dlib.shape_predictor("5_points.dat")
img = cv2.imread(sys.argv[1])
faces = detector(img,1)
if (len(faces) > 0):
    for k,d in enumerate(faces):
    #   cv2.rectangle(img,(d.left(),d.top()),(d.right(),d.bottom()),(255,255,255))
        shape = landmark_predictor(img,d)
        for i in range(1,4):
            #cv2.circle(img, (shape.part(i).x, shape.part(i).y),1,(0,255,0), -1, 8)
            #cv2.putText(img,str(i),(shape.part(i).x,shape.part(i).y),cv2.FONT_HERSHEY_SIMPLEX,0.5,(255,2555,255))
            print(shape.part(i).x)
            print(shape.part(i).y)
            #print("\n")
            #print(shape.part(i).y)
            #print("\n")

#cv2.imshow('Frame', img)
#cv2.imwrite("E:\\Data\\withlight\\alreadytest\\13.jpg", img)
#cv2.waitKey(0)