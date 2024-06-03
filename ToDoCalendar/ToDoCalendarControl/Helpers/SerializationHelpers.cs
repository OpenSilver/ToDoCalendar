using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

using System.Xml;

namespace ToDoCalendarControl
{
    static class SerializationHelpers
    {
        public static ObjectType Deserialize<ObjectType>(string xml) where ObjectType : class
        {
            if (!string.IsNullOrEmpty(xml))
            {
                //// ----------- XML SERIALIZER VERSION: -----------
                //using (System.IO.StringReader reader = new System.IO.StringReader(xml))
                //{
                //    XmlSerializer valueSerializer = new XmlSerializer(typeof(ObjectType));
                //    return valueSerializer.Deserialize(reader) as ObjectType;
                //}

                // ----------- DATA CONTRACT SERIALIZER VERSION: -----------
                var serializer = new DataContractSerializer(typeof(ObjectType));
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
                {
                    return (ObjectType)serializer.ReadObject(ms);
                }
            }
            else
            {
                return null;
            }
        }

        public static string Serialize(object objectToSerialize)
        {
            //// ----------- XML SERIALIZER VERSION: -----------
            //using (System.IO.StringWriter stringWriter = new System.IO.StringWriter())
            //{
            //    XmlSerializer xmlSerializer = new XmlSerializer(objectToSerialize.GetType());
            //    xmlSerializer.Serialize(stringWriter, objectToSerialize);
            //    return stringWriter.ToString();
            //}

            // ----------- DATA CONTRACT SERIALIZER VERSION: -----------
            using (MemoryStream ms = new MemoryStream())
            {
                var serializer = new DataContractSerializer(objectToSerialize.GetType());
                serializer.WriteObject(ms, objectToSerialize);
                return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Position);
            }
        }
    }
}
