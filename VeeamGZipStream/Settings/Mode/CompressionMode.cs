using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VeeamGZipStream.Settings.Mode.Instructions;

namespace VeeamGZipStream.Settings.Mode
{
    public class Mode
    {
        #region Fields

        public static readonly Mode COMPRESS = new Mode("compress", new CompressInstruction());
        public static readonly Mode DECOMPRESS = new Mode("decompress", new DecompressInstruction());

        private string operation;
        private IGZipInstruction instruction;

        #endregion Fields

        #region .ctor

        Mode(string operation, IGZipInstruction instruction)
        {
            this.operation = operation;
            this.instruction = instruction;
        }

        #endregion .ctor

        #region Properties

        public static IEnumerable<Mode> Values
        {
            get
            {
                yield return COMPRESS;
                yield return DECOMPRESS;
            }
        }

        #endregion

        public string Operation
        {
            get
            {
                return operation;
            }
        }

        public IGZipInstruction Instruction
        {
            get
            {
                return instruction;
            }
        }

        public static Mode GetByName(string operation)
        {
            try
            {
                return Mode.Values.First((mode) => mode.operation == operation);
            }
            catch(ArgumentNullException argEx)
            {
                throw new ArgumentNullException("Значение параметра операции (compress/decompress) задано неверно.\n" + argEx.Message);
            }
            catch (InvalidOperationException invalidEx)
            {
                throw new InvalidOperationException("Ни один элемент не удовлетворяет условию предиката поиска операции (compress/decompress). \n" + 
                    invalidEx.Message);
            }
        }
    }
}
