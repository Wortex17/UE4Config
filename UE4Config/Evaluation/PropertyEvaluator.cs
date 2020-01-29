using System;
using System.Collections.Generic;
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
        public void EvaluateInstructions(List<InstructionToken> instructions, List<string> values)
        {
            throw new NotImplementedException();
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
        public void EvaluatePropertyValues(List<ConfigIni> configs, string sectionName, string propertyKey,
            List<InstructionToken> instructions, List<string> values)
        {
            foreach (var configIni in configs)
            {
                configIni.FindPropertyInstructions(sectionName, propertyKey, instructions);
            }
            EvaluateInstructions(instructions, values);
        }

        public void EvaluatePropertyValues(List<ConfigIni> configs, string sectionName, string propertyKey, List<string> values)
        {
            EvaluatePropertyValues(configs, sectionName, propertyKey, new List<InstructionToken>(), values);
        }
    }
}