CFLAGS = -O2 -Wall -Wpedantic -fPIC

light-modbus.so: light-modbus.o light-modbus-rtu.o emi-utils.o
	$(CC) $(CFLAGS) -shared -o light-modbus.so light-modbus.o light-modbus-rtu.o emi-utils.o

light-modbus.o: light-modbus.c light-modbus.h
	$(CC) $(CFLAGS) -c light-modbus.c -o light-modbus.o

light-modbus-rtu.o: light-modbus-rtu.c light-modbus-rtu.h
	$(CC) $(CFLAGS) -c light-modbus-rtu.c -o light-modbus-rtu.o

emi-utils.o: emi-utils.c emi-utils.h
	$(CC) $(CFLAGS) -lm -c emi-utils.c -o emi-utils.o

.PHONY: clean

clean:
	rm -rf *.o *.so
