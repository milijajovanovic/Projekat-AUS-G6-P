using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus write single register functions/requests.
    /// </summary>
    public class WriteSingleRegisterFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteSingleRegisterFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public WriteSingleRegisterFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusWriteCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            ModbusWriteCommandParameters p = CommandParameters as ModbusWriteCommandParameters;
            byte[] request = new byte[12];

            // Transaction ID
            request[0] = (byte)(p.TransactionId >> 8);
            request[1] = (byte)(p.TransactionId);
            // Protocol ID = 0
            request[2] = 0;
            request[3] = 0;
            // Length = 6
            request[4] = 0;
            request[5] = 6;
            // Unit ID
            request[6] = p.UnitId;
            // Function Code
            request[7] = p.FunctionCode;
            // Output Address
            request[8] = (byte)(p.OutputAddress >> 8);
            request[9] = (byte)(p.OutputAddress);
            // Vrednost registra
            request[10] = (byte)(p.Value >> 8);
            request[11] = (byte)(p.Value);

            return request;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            ModbusWriteCommandParameters p = CommandParameters as ModbusWriteCommandParameters;
            Dictionary<Tuple<PointType, ushort>, ushort> result = new Dictionary<Tuple<PointType, ushort>, ushort>();

            if (response == null || response.Length < 12)
                throw new ArgumentException("Invalid response length.");

            if (response[7] == p.FunctionCode + 0x80)
            {
                HandeException(response[8]);
            }

            // Odgovor je echo zahteva, vrednost je na bajtovima 10-11
            ushort value = (ushort)((response[10] << 8) | response[11]);
            result.Add(new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, p.OutputAddress), value);

            return result;
        }
    }
}