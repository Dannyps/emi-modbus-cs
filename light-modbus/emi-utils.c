#include "emi-utils.h"

#include <stdlib.h>
#include <math.h>

double scaleInt(int num, int scaler)
{
    if (scaler == 0)
    {
        // No effect
        return num;
    }
    else
    {
        return num * pow(10, scaler);
    }
}

int getDoubleFromUInt16(modbus_t *ctx, uint16_t registerAddress, signed char scaler, double *res)
{
    uint16_t buffer;
    int rc = modbus_read_input_registers(ctx, registerAddress, 1, 2, &buffer);
    *res = scaleInt(__bswap_16(buffer), scaler);
    return rc;
}

int getDoubleFromUInt32(modbus_t *ctx, uint16_t registerAddress, signed char scaler, double *res)
{
    uint32_t buffer;
    int rc = modbus_read_input_registers(ctx, registerAddress, 1, 4, &buffer);
    *res = scaleInt(__bswap_32(buffer), scaler);
    return rc;
}

emi_clock_t *getTime(modbus_t *ctx)
{
    emi_clock_t *emiClock = malloc(1 * sizeof(emi_clock_t));
    // sizeof(emi_clock_t) is 13 bytes, which spans 7 Modbus registers (13 bytes / 2 bytes per register, rounded up)
    int nb = (sizeof(emi_clock_t) + 1) / 2;
    int rc = modbus_read_input_registers(ctx, 0x0001, nb, sizeof(emi_clock_t), emiClock);
    if (rc == -1)
    {
        free(emiClock);
        return NULL; // Error reading registers
    }
    emiClock->year = __bswap_16(emiClock->year);
    emiClock->deviation = __bswap_16(emiClock->deviation);
    return emiClock;
}

int getUnsignedFromInt8(modbus_t *ctx, uint16_t registerAddress, unsigned char *res)
{
    int8_t buffer;
    int rc = modbus_read_input_registers(ctx, registerAddress, 1, 1, &buffer);
    *res = (unsigned char)buffer;
    return rc;
}

char* getOctetString(modbus_t* ctx, uint16_t registerAddress, uint8_t nb)
{
    // Allocate buffer for the octet string (nb bytes + null terminator)
    char* octetString = malloc((nb + 1) * sizeof(char));
    if (octetString == NULL)
    {
        return NULL; // Memory allocation failed
    }
    
    // Calculate number of registers needed (nb bytes / 2 bytes per register, rounded up)
    int numRegisters = (nb + 1) / 2;
    int rc = modbus_read_input_registers(ctx, registerAddress, numRegisters, nb, octetString);
    
    if (rc == -1)
    {
        free(octetString);
        return NULL; // Error reading registers
    }
    
    // Null-terminate the string
    octetString[nb] = '\0';
    return octetString;
}

void freeTime(emi_clock_t *emiClock)
{
    if (emiClock != NULL)
    {
        free(emiClock);
    }
}

void freeOctetString(char *octetString)
{
    if (octetString != NULL)
    {
        free(octetString);
    }
}