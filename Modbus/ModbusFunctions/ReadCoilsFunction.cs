using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read coil functions/requests.
    /// </summary>
    public class ReadCoilsFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadCoilsFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
		public ReadCoilsFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc/>
        public override byte[] PackRequest()
        {
            ModbusReadCommandParameters parameters = CommandParameters as ModbusReadCommandParameters;
            byte[] requestBytes = new byte[12];

            requestBytes[0] = (byte)(parameters.TransactionId >> 8);
            requestBytes[1] = (byte)(parameters.TransactionId);
            requestBytes[2] = 0;
            requestBytes[3] = 0;
            requestBytes[4] = 0;
            requestBytes[5] = 6;
            requestBytes[6] = parameters.UnitId;
            requestBytes[7] = parameters.FunctionCode;
            requestBytes[8] = (byte)(parameters.StartAddress >> 8);
            requestBytes[9] = (byte)(parameters.StartAddress);
            requestBytes[10] = (byte)(parameters.Quantity >> 8);
            requestBytes[11] = (byte)(parameters.Quantity);

            return requestBytes;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            ModbusReadCommandParameters parameters = CommandParameters as ModbusReadCommandParameters;
            Dictionary<Tuple<PointType, ushort>, ushort> coilStates = new Dictionary<Tuple<PointType, ushort>, ushort>();

            if (response[7] == parameters.FunctionCode + 0x80)
            {
                HandeException(response[8]);
            }

            for (int i = 0; i < parameters.Quantity; i++)
            {
                int targetByteIndex = 9 + (i / 8);
                int targetBitIndex = i % 8;
                ushort state = (ushort)((response[targetByteIndex] >> targetBitIndex) & 1);
                coilStates.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, (ushort)(parameters.StartAddress + i)), state);
            }

            return coilStates;
        }
    }
}