using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read holding registers functions/requests.
    /// </summary>
    public class ReadHoldingRegistersFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadHoldingRegistersFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public ReadHoldingRegistersFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            ModbusReadCommandParameters p = CommandParameters as ModbusReadCommandParameters;
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
            // Start Address
            request[8] = (byte)(p.StartAddress >> 8);
            request[9] = (byte)(p.StartAddress);
            // Quantity
            request[10] = (byte)(p.Quantity >> 8);
            request[11] = (byte)(p.Quantity);

            return request;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            ModbusReadCommandParameters p = CommandParameters as ModbusReadCommandParameters;
            Dictionary<Tuple<PointType, ushort>, ushort> result = new Dictionary<Tuple<PointType, ushort>, ushort>();

            if (response == null || response.Length < 9)
                throw new ArgumentException("Invalid response length.");

            if (response[7] == p.FunctionCode + 0x80)
            {
                HandeException(response[8]);
            }

            // Podaci počinju od indexa 9, svaki registar je 2 bajta, big endian
            for (int i = 0; i < p.Quantity; i++)
            {
                ushort value = (ushort)((response[9 + 2 * i] << 8) | response[10 + 2 * i]);
                result.Add(new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, (ushort)(p.StartAddress + i)), value);
            }

            return result;
        }
    }
}