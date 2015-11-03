//// OsmSharp - OpenStreetMap (OSM) SDK
//// Copyright (C) 2015 Abelshausen Ben
//// 
//// This file is part of OsmSharp.
//// 
//// OsmSharp is free software: you can redistribute it and/or modify
//// it under the terms of the GNU General Public License as published by
//// the Free Software Foundation, either version 2 of the License, or
//// (at your option) any later version.
//// 
//// OsmSharp is distributed in the hope that it will be useful,
//// but WITHOUT ANY WARRANTY; without even the implied warranty of
//// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//// GNU General Public License for more details.
//// 
//// You should have received a copy of the GNU General Public License
//// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

//using System.Collections.Generic;

//namespace OsmSharp.Routing.Transit.Multimodal.Instructions.Modal
//{
//    /// <summary>
//    /// A language generator expanded with some extra transit instructions.
//    /// </summary>
//    public class ModalLanguageGenerator : OsmSharp.Routing.Instructions.LanguageGeneration.ILanguageGenerator
//    {
//        /// <summary>
//        /// Generates an instruction for the given data.
//        /// </summary>
//        /// <param name="instructionData"></param>
//        /// <param name="text"></param>
//        /// <returns></returns>
//        public bool Generate(Dictionary<string, object> instructionData, out string text)
//        {
//            string type;
//            if (instructionData.TryGetValue("osmsharp.instruction.type", out type))
//            {
//                if (type == "transfer")
//                {
//                    text = "Transfer";
//                    return true;
//                }
//                else if (type == "transit")
//                {
//                    text = "Transit";
//                    return true;
//                }
//                else if (type == "anything_but_transit")
//                {
//                    text = "No transit";
//                    return true;
//                }
//            }
//            text = string.Empty;
//            return false;
//        }
//    }
//}
