using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace App.Domain
{

    public enum GrammaticalClassEnum
    {
        Substantivo,
        Artigo,
        Adjetivo,
        Numeral,
        Pronome,
        Verbo,
        Advérbio,
        Preposição,
        Conjunção,
        Interjeição,
    }

    public enum GenderVariationEnum
    {
        Masculino,
        Feminino,
    }

    public enum NumberVariationEnum
    {
        Singular,
        Plural,
    }

    [DataContract]
    [XmlRoot]
    public class Grammar
    {
        [DataMember]
        [XmlElement]
        public GrammaticalClassEnum GrammaticalClass { get; set; }

        [DataMember]
        [XmlElement]
        public GenderVariationEnum GenderVariation { get; set; }

        [DataMember]
        [XmlElement]
        public NumberVariationEnum NumberVariation { get; set; }
    }

    [DataContract]
    [XmlRoot]
    public class FriendlyGrammar
    {
        [DataMember]
        [XmlElement]
        public string GrammaticalClass { get; set; }

        [DataMember]
        [XmlElement]
        public string GenderVariation { get; set; }

        [DataMember]
        [XmlElement]
        public string NumberVariation { get; set; }
    }

    [DataContract]
    [XmlRoot]
    public class Word
    {
        [DataMember]
        [XmlElement]
        public String Token { get; set; }

        [IgnoreDataMember]
        [XmlIgnore]
        public Dictionary<GrammaticalClassEnum, Grammar> GrammaticalClass { get; set; }

        [DataMember]
        [XmlElement]
        public string GrammaticalClassAsJson { get; set; }
        
        [DataMember]
        [XmlElement]
        public Dictionary<string, FriendlyGrammar> FriendlyGrammaticalClass { get; set; }
    }

    [DataContract]
    [XmlRoot]
    public class WordRepository
    {
        [DataMember]
        [XmlElement]
        public List<Word> WordList { get; set; }
    }
}
