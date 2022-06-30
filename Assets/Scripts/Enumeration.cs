using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace gpredict3_gaming.Ikaros
{
    /// <summary>
    /// Generic class for representation of a list of options and corresponding codes
    /// </summary>
    /// <typeparam name="TName">option</typeparam>
    /// <typeparam name="TCode">code</typeparam>
    public class Enumeration<TName, TCode>
    {
        public List<TName> Names { get; private set; }
        public List<TCode> Codes { get; private set; }

        public Enumeration()
        {
            Names = new List<TName>();
            Codes = new List<TCode>();
        }

        public void Add(TName name, TCode code)
        {
            Names.Add(name);
            Codes.Add(code);
        }

        public TName[] NamesToArray()
        {
            return Names.ToArray();
        }

        public TCode[] CodesToArray()
        {
            return Codes.ToArray();
        }
    }
}
