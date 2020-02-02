using System.Collections.Generic;
using System.ComponentModel;
using UE4Config.Parsing;

namespace UE4Config.Evaluation
{
    public class PropertyEvaluator
    {
        /// <summary>
        /// Returns a default already instanced PropertyEvaluater for easy use.
        /// Instances a new default <see cref="PropertyEvaluator"/> if there was none set yet.
        /// </summary>
        public static PropertyEvaluator Default
        {
            get
            {
                if (m_Default == null)
                {
                    m_Default = new PropertyEvaluator();
                }
                return m_Default;
            }
        }

        /// <summary>
        /// Sets the <see cref="PropertyEvaluator" /> instance to eb returned by <see cref="Default"/>
        /// </summary>
        public static void SetDefaultEvaluator(PropertyEvaluator evaluator)
        {
            m_Default = evaluator;
        }

        private static PropertyEvaluator m_Default;

        /// <summary>
        /// Executes ordered list of instructions, assuming they are for the same property, modifying its <see cref="propertyValues"/> in the progress.
        /// <seealso cref="ExecutePropertyInstruction"/>
        /// </summary>
        /// <param name="instructions">Queue of instructions to be executed</param>
        /// <param name="propertyValues">The values of the property the instructions will be executed for. Will be modified by execution.</param>
        public void ExecutePropertyInstructions(IList<InstructionToken> instructions, IList<string> propertyValues)
        {
            foreach (var instruction in instructions)
            {
                if (instruction != null)
                {
                    ExecutePropertyInstruction(instruction, propertyValues);
                }
            }
        }

        /// <summary>
        /// Executes a single instruction, modifying the <see cref="values"/> of a property.
        /// </summary>
        /// <param name="instruction">The instruction to be executed</param>
        /// <param name="propertyValues">The values of the property the instruction will be executed for. Will be modified by execution.</param>
        public void ExecutePropertyInstruction(InstructionToken instruction, IList<string> propertyValues)
        {
            switch (instruction.InstructionType)
            {
                case InstructionType.Add:
                    if (!propertyValues.Contains(instruction.Value))
                    {
                        propertyValues.Add(instruction.Value);
                    }
                    break;
                case InstructionType.AddForce:
                    propertyValues.Add(instruction.Value);
                    break;
                case InstructionType.Remove:
                    bool wasRemoved = false;
                    do
                    {
                        wasRemoved = propertyValues.Remove(instruction.Value);
                    } while (wasRemoved);
                    break;
                case InstructionType.RemoveAll:
                    propertyValues.Clear();
                    break;
                case InstructionType.Set:
                    propertyValues.Clear();
                    propertyValues.Add(instruction.Value);
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
            ExecutePropertyInstructions(instructions, values);
        }

        public void EvaluatePropertyValues(IEnumerable<ConfigIni> configs, string sectionName, string propertyKey, IList<string> values)
        {
            EvaluatePropertyValues(configs, sectionName, propertyKey, new List<InstructionToken>(), values);
        }

        public void EvaluatePropertyValues(ConfigIni config, string sectionName, string propertyKey,
            IList<InstructionToken> instructions, IList<string> values)
        {
            config.FindPropertyInstructions(sectionName, propertyKey, instructions);
            ExecutePropertyInstructions(instructions, values);
        }

        public void EvaluatePropertyValues(ConfigIni configs, string sectionName, string propertyKey, IList<string> values)
        {
            EvaluatePropertyValues(configs, sectionName, propertyKey, new List<InstructionToken>(), values);
        }
    }
}