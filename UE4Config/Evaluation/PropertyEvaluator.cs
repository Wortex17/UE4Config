using System;
using System.Collections.Generic;
using System.ComponentModel;
using UE4Config.Parser;

namespace UE4Config.Evaluation
{
    public class PropertyEvaluator
    {
        /// <remarks>
        /// STUB
        /// </remarks>
        /// <summary>
        /// Evaluates an ordered list of instructions 
        /// </summary>
        /// <param name="instructions">Ordered list of instructions, with the latest declaration being last</param>
        /// <param name="values">Resulting list of values, possibly empty if the property was deleted</param>
        public void EvaluateInstructions(IEnumerable<InstructionToken> instructions, IList<string> values)
        {
            foreach (var instruction in instructions)
            {
                if (instruction != null)
                {
                    EvaluateSingleInstruction(instruction, values);
                }
            }
        }

        public void EvaluateSingleInstruction(InstructionToken instruction, IList<string> values)
        {
            switch (instruction.InstructionType)
            {
                case InstructionType.Add:
                case InstructionType.AddOverride:
                    break;
                case InstructionType.Remove:
                    break;
                case InstructionType.RemoveAll:
                    break;
                case InstructionType.Set:
                    values.Clear();
                    values.Add(instruction.Value);
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(instruction.InstructionType), (int)instruction.InstructionType, typeof(InstructionType));
            }
        }

        /// <remarks>
        /// STUB
        /// </remarks>
        /// <summary>
        /// Evaluates the value of an property, as constructed by one or more <see cref="ConfigIni"/>s
        /// </summary>
        /// <param name="configs">Ordered list of configs, with the latest / highest priority declarations being last</param>
        /// <param name="sectionName">The section the property should be fetched from</param>
        /// <param name="propertyKey">The property key (name) to fetch</param>
        /// <param name="instructions">Cache for the list of instructions regarding this property. Will be added in order of declaration, the latest one being last</param>
        /// <param name="values">Resulting list of values, possibly empty if the property was deleted</param>
        public void EvaluatePropertyValues(IEnumerable<ConfigIni> configs, string sectionName, string propertyKey,
            IList<InstructionToken> instructions, IList<string> values)
        {
            foreach (var configIni in configs)
            {
                configIni.FindPropertyInstructions(sectionName, propertyKey, instructions);
            }
            EvaluateInstructions(instructions, values);
        }

        public void EvaluatePropertyValues(IEnumerable<ConfigIni> configs, string sectionName, string propertyKey, IList<string> values)
        {
            EvaluatePropertyValues(configs, sectionName, propertyKey, new List<InstructionToken>(), values);
        }
    }
}