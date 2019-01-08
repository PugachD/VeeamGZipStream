using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestVeeamGZipStream.Settings.Mode.Instructions;

namespace TestVeeamGZipStream.Settings.Mode
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
                Console.WriteLine("Значение параметра операции задано неверно");
                throw argEx;
            }
            catch (InvalidOperationException invalidEx)
            {
                Console.WriteLine("Ни один элемент не удовлетворяет условию предиката");
                throw invalidEx;
            }
        }
    }
}
