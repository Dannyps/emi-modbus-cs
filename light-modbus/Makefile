CFLAGS = -O2 -Wall -Wpedantic

build: light-modbus.o light-modbus-rtu.o 
	$(CC) $(CFLAGS) -shared -o light-modbus.so light-modbus.o light-modbus-rtu.o

light-modbus.o: light-modbus.c light-modbus.h
	$(CC) $(CFLAGS) -fPIC -c light-modbus.c -o light-modbus.o

light-modbus-rtu.o: light-modbus-rtu.c light-modbus-rtu.h
	$(CC) $(CFLAGS) -fPIC -c light-modbus-rtu.c -o light-modbus-rtu.o

.PHONY: clean build

clean:
	rm -rf *.o *.so
