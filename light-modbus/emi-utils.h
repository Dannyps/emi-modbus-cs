#include "light-modbus-rtu.h"

/**
 * @brief Clock structure for EMI devices.
 * This structure is packed to ensure that it has no padding bytes,
 * which is important for correct data alignment when reading from Modbus registers.
 */
typedef struct __attribute__ ((__packed__)) {
    uint16_t year;
    uint8_t month;
    uint8_t day;
    uint8_t weekday;
    uint8_t hour;
    uint8_t minute;
    uint8_t second;
    uint8_t hundredthOfSecond;
    uint16_t deviation;
    uint8_t clockStatus;
} emi_clock_t;

/**
 * @brief Load an Octet String from a Modbus register
 * 
 * @param ctx the modbus context.
 * @param registerAddress the register address to read from
 * @param nb the number of octets to read
 * 
 * @return a pointer to the octet string read from the Modbus device.
 * 
 * @warning The caller is responsible for freeing the returned string using freeOctetString().
 */
char* getOctetString(modbus_t* ctx, uint16_t registerAddress, uint8_t nb);

/**
 * @brief Load a Long Unsigned Integer from a Modbus register
 * 
 * @param ctx the modbus context.
 * @param registerAddress the register address to read from
 * @param scaller the scaller to apply (* 10 ^ {scaller})
 * @param res the palce to store the result
 * @return the return value from the inner modbus_read_input_registers call.
 * 
 * @warning This function assumes that the Modbus register contains a 16-bit unsigned integer.
 *          Keep in mind that the return value is not zero on success.
 */
int getDoubleFromUInt16(modbus_t* ctx, uint16_t registerAddress, signed char scaller, double* res);

/**
 * @brief Load a Double Long Unsigned Integer from a Modbus register
 * 
 * @param ctx the modbus context.
 * @param registerAddress the register address to read from
 * @param scaller the scaller to apply (* 10 ^ {scaller})
 * @param res the palce to store the result
 * @return the return value from the inner modbus_read_input_registers call.
 * 
 * @warning This function assumes that the Modbus register contains a 32-bit unsigned integer.
 *          Keep in mind that the return value is not zero on success.
 */
int getDoubleFromUInt32(modbus_t* ctx, uint16_t registerAddress, signed char scaler, double* res);

/**
 * @brief Get the current time from the Modbus device.
 * 
 * @param ctx the modbus context.
 * @return a pointer to an emi_clock_t structure containing the time information.
 * 
 * @warning The returned pointer should be freed using freeTime() after use.
 */
emi_clock_t* getTime(modbus_t* ctx);

/**
 * @brief Free the memory allocated for an emi_clock_t structure.
 * 
 * @param emiClock pointer to the emi_clock_t structure to free.
 */
void freeTime(emi_clock_t *emiClock);

/**
 * @brief Free the memory allocated for an octet string.
 * 
 * @param octetString pointer to the octet string to free.
 */
void freeOctetString(char *octetString);