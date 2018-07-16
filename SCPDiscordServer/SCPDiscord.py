from socket import *;

port = 8888
serverSocket = socket(AF_INET, SOCK_STREAM)

serverSocket.bind(('',port))
serverSocket.listen(1)
print ('Bot server online')
connectionSocket, addr = serverSocket.accept()
print ('Connection established')

while 1:
    try:
        binData = connectionSocket.recv(10025)
        data = binData.decode();
        if data == "":
            raise ConnectionAbortedError()
        print(data)
    except ConnectionAbortedError as error:
        print ('Connection broken')
        connectionSocket, addr = serverSocket.accept()
        print ('Connection established')
        
