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
    int rc = modbus_read_input_registers(ctx, 0x0001, 1, sizeof(emi_clock_t), emiClock);
    if (rc != 0)
    {
    }
    emiClock->year = __bswap_16(emiClock->year);
    emiClock->deviation = __bswap_16(emiClock->deviation);
    return emiClock;
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