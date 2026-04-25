using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus write coil functions/requests.
    /// </summary>
    public class WriteSingleCoilFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteSingleCoilFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public WriteSingleCoilFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
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
            // Vrednost: 0xFF00 = ON, 0x0000 = OFF
            request[10] = (byte)(p.Value == 1 ? 0xFF : 0x00);
            request[11] = 0x00;

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
            ushort value = (ushort)(response[10] == 0xFF ? 1 : 0);
            result.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, p.OutputAddress), value);

            return result;
        }
    }
}