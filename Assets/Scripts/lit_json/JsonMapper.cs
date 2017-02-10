#region Header
/**
 * JsonMapper.cs
 *   JSON to .Net object and object to JSON conversions.
 *
 * The authors disclaim copyright to this source code. For more details, see
 * the COPYING file included with this distribution.
 **/
#endregion


using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;


namespace LitJsonSrc
{

    internal delegate void ExporterFunc    (object obj, JsonWriter writer);
    public   delegate void ExporterFunc<T> (T obj, JsonWriter writer);

    internal delegate object ImporterFunc                (object input);
    public   delegate TValue ImporterFunc<TJson, TValue> (TJson input);

    public delegate IJsonWrapper WrapperFactory ();


    public class JsonMapper
    {
        #region Fields
        private static int max_nesting_depth;

        private static IFormatProvider datetime_format;

        private static IDictionary<Type, ExporterFunc> base_exporters_table;
        private static IDictionary<Type, ExporterFunc> custom_exporters_table;

        private static IDictionary<Type,
                IDictionary<Type, ImporterFunc>> base_importers_table;
        private static IDictionary<Type,
                IDictionary<Type, ImporterFunc>> custom_importers_table;

        private static IDictionary<Type,
                IDictionary<Type, MethodInfo>> conv_ops;

        private static JsonWriter      static_writer;
        private static readonly object static_writer_lock = new Object ();
        #endregion


        #region Constructors
        static JsonMapper ()
        {
            max_nesting_depth = 100;

            static_writer = new JsonWriter ();

            datetime_format = DateTimeFormatInfo.InvariantInfo;

            base_exporters_table   = new Dictionary<Type, ExporterFunc> ();
            custom_exporters_table = new Dictionary<Type, ExporterFunc> ();

            base_importers_table = new Dictionary<Type,
                                 IDictionary<Type, ImporterFunc>> ();
            custom_importers_table = new Dictionary<Type,
                                   IDictionary<Type, ImporterFunc>> ();

            RegisterBaseExporters ();
            RegisterBaseImporters ();
        }
        #endregion


        #region Private Methods

        private static IJsonWrapper ReadValue (WrapperFactory factory,
                                               JsonReader reader)
        {
            reader.Read ();

            if (reader.Token == JsonToken.ArrayEnd ||
                reader.Token == JsonToken.Null)
                return null;

            IJsonWrapper instance = factory ();

            if (reader.Token == JsonToken.String) {
                instance.SetString ((string) reader.Value);
                return instance;
            }

            if (reader.Token == JsonToken.Double) {
                instance.SetDouble ((double) reader.Value);
                return instance;
            }

            if (reader.Token == JsonToken.Int) {
                instance.SetInt ((int) reader.Value);
                return instance;
            }

            if (reader.Token == JsonToken.Long) {
                instance.SetLong ((long) reader.Value);
                return instance;
            }

            if (reader.Token == JsonToken.Boolean) {
                instance.SetBoolean ((bool) reader.Value);
                return instance;
            }

            if (reader.Token == JsonToken.ArrayStart) {
                instance.SetJsonType (JsonType.Array);

                while (true) {
                    IJsonWrapper item = ReadValue (factory, reader);
                    if (item == null && reader.Token == JsonToken.ArrayEnd)
                        break;

                    ((IList) instance).Add (item);
                }
            }
            else if (reader.Token == JsonToken.ObjectStart) {
                instance.SetJsonType (JsonType.Object);

                while (true) {
                    reader.Read ();

                    if (reader.Token == JsonToken.ObjectEnd)
                        break;

                    string property = (string) reader.Value;

                    ((IDictionary) instance)[property] = ReadValue (
                        factory, reader);
                }

            }

            return instance;
        }

        private static void ReadSkip (JsonReader reader)
        {
            ToWrapper (
                delegate { return new JsonMockWrapper (); }, reader);
        }

        private static void RegisterBaseExporters ()
        {
            base_exporters_table[typeof (byte)] =
                delegate (object obj, JsonWriter writer) {
                    writer.Write (Convert.ToInt32 ((byte) obj));
                };

            base_exporters_table[typeof (char)] =
                delegate (object obj, JsonWriter writer) {
                    writer.Write (Convert.ToString ((char) obj));
                };

            base_exporters_table[typeof (DateTime)] =
                delegate (object obj, JsonWriter writer) {
                    writer.Write (Convert.ToString ((DateTime) obj,
                                                    datetime_format));
                };

            base_exporters_table[typeof (decimal)] =
                delegate (object obj, JsonWriter writer) {
                    writer.Write ((decimal) obj);
                };

            base_exporters_table[typeof (sbyte)] =
                delegate (object obj, JsonWriter writer) {
                    writer.Write (Convert.ToInt32 ((sbyte) obj));
                };

            base_exporters_table[typeof (short)] =
                delegate (object obj, JsonWriter writer) {
                    writer.Write (Convert.ToInt32 ((short) obj));
                };

            base_exporters_table[typeof (ushort)] =
                delegate (object obj, JsonWriter writer) {
                    writer.Write (Convert.ToInt32 ((ushort) obj));
                };

            base_exporters_table[typeof (uint)] =
                delegate (object obj, JsonWriter writer) {
                    writer.Write (Convert.ToUInt64 ((uint) obj));
                };

            base_exporters_table[typeof (ulong)] =
                delegate (object obj, JsonWriter writer) {
                    writer.Write ((ulong) obj);
                };
        }

        private static void RegisterBaseImporters ()
        {
            ImporterFunc importer;

            importer = delegate (object input) {
                return Convert.ToByte ((int) input);
            };
            RegisterImporter (base_importers_table, typeof (int),
                              typeof (byte), importer);

            importer = delegate (object input) {
                return Convert.ToUInt64 ((int) input);
            };
            RegisterImporter (base_importers_table, typeof (int),
                              typeof (ulong), importer);

            importer = delegate (object input) {
                return Convert.ToSByte ((int) input);
            };
            RegisterImporter (base_importers_table, typeof (int),
                              typeof (sbyte), importer);

            importer = delegate (object input) {
                return Convert.ToInt16 ((int) input);
            };
            RegisterImporter (base_importers_table, typeof (int),
                              typeof (short), importer);

            importer = delegate (object input) {
                return Convert.ToUInt16 ((int) input);
            };
            RegisterImporter (base_importers_table, typeof (int),
                              typeof (ushort), importer);

            importer = delegate (object input) {
                return Convert.ToUInt32 ((int) input);
            };
            RegisterImporter (base_importers_table, typeof (int),
                              typeof (uint), importer);

            importer = delegate (object input) {
                return Convert.ToSingle ((int) input);
            };
            RegisterImporter (base_importers_table, typeof (int),
                              typeof (float), importer);

            importer = delegate (object input) {
                return Convert.ToDouble ((int) input);
            };
            RegisterImporter (base_importers_table, typeof (int),
                              typeof (double), importer);

            importer = delegate (object input) {
                return Convert.ToDecimal ((double) input);
            };
            RegisterImporter (base_importers_table, typeof (double),
                              typeof (decimal), importer);


            importer = delegate (object input) {
                return Convert.ToUInt32 ((long) input);
            };
            RegisterImporter (base_importers_table, typeof (long),
                              typeof (uint), importer);

            importer = delegate (object input) {
                return Convert.ToChar ((string) input);
            };
            RegisterImporter (base_importers_table, typeof (string),
                              typeof (char), importer);

            importer = delegate (object input) {
                return Convert.ToDateTime ((string) input, datetime_format);
            };
            RegisterImporter (base_importers_table, typeof (string),
                              typeof (DateTime), importer);
        }

        private static void RegisterImporter (
            IDictionary<Type, IDictionary<Type, ImporterFunc>> table,
            Type json_type, Type value_type, ImporterFunc importer)
        {
            if (! table.ContainsKey (json_type))
                table.Add (json_type, new Dictionary<Type, ImporterFunc> ());

            table[json_type][value_type] = importer;
        }

        private static void WriteValue (object obj, JsonWriter writer,
                                        bool writer_is_private,
                                        int depth)
        {
            if (depth > max_nesting_depth)
                throw new JsonException (
                    String.Format ("Max allowed object depth reached while " +
                                   "trying to export from type {0}",
                                   obj.GetType ()));

            if (obj == null) {
                writer.Write (null);
                return;
            }

            if (obj is IJsonWrapper) {
                if (writer_is_private)
                    writer.TextWriter.Write (((IJsonWrapper) obj).ToJson ());
                else
                    ((IJsonWrapper) obj).ToJson (writer);

                return;
            }

            if (obj is String) {
                writer.Write ((string) obj);
                return;
            }

            if (obj is Double) {
                writer.Write ((double) obj);
                return;
            }

            if (obj is Int32) {
                writer.Write ((int) obj);
                return;
            }

            if (obj is Boolean) {
                writer.Write ((bool) obj);
                return;
            }

            if (obj is Int64) {
                writer.Write ((long) obj);
                return;
            }

            if (obj is Array) {
                writer.WriteArrayStart ();

                foreach (object elem in (Array) obj)
                    WriteValue (elem, writer, writer_is_private, depth + 1);

                writer.WriteArrayEnd ();

                return;
            }

            if (obj is IList) {
                writer.WriteArrayStart ();
                foreach (object elem in (IList) obj)
                    WriteValue (elem, writer, writer_is_private, depth + 1);
                writer.WriteArrayEnd ();

                return;
            }

            if (obj is IDictionary) {
                writer.WriteObjectStart ();
                foreach (DictionaryEntry entry in (IDictionary) obj) {
                    writer.WritePropertyName ((string) entry.Key);
                    WriteValue (entry.Value, writer, writer_is_private,
                                depth + 1);
                }
                writer.WriteObjectEnd ();

                return;
            }

            Type obj_type = obj.GetType ();

            // See if there's a custom exporter for the object
            if (custom_exporters_table.ContainsKey (obj_type)) {
                ExporterFunc exporter = custom_exporters_table[obj_type];
                exporter (obj, writer);

                return;
            }

            // If not, maybe there's a base exporter
            if (base_exporters_table.ContainsKey (obj_type)) {
                ExporterFunc exporter = base_exporters_table[obj_type];
                exporter (obj, writer);

                return;
            }

            // Last option, let's see if it's an enum
            if (obj is Enum) {
                Type e_type = Enum.GetUnderlyingType (obj_type);

                if (e_type == typeof (long)
                    || e_type == typeof (uint)
                    || e_type == typeof (ulong))
                    writer.Write ((ulong) obj);
                else
                    writer.Write ((int) obj);

                return;
            }
        }
        #endregion


        public static string ToJson (object obj)
        {
            lock (static_writer_lock) {
                static_writer.Reset ();

                WriteValue (obj, static_writer, true, 0);

                return static_writer.ToString ();
            }
        }

        public static void ToJson (object obj, JsonWriter writer)
        {
            WriteValue (obj, writer, false, 0);
        }

        public static JsonData ToObject (JsonReader reader)
        {
            return (JsonData) ToWrapper (
                delegate { return new JsonData (); }, reader);
        }

        public static JsonData ToObject (TextReader reader)
        {
            JsonReader json_reader = new JsonReader (reader);

            return (JsonData) ToWrapper (
                delegate { return new JsonData (); }, json_reader);
        }

        public static JsonData ToObject (string json)
        {
            return (JsonData) ToWrapper (
                delegate { return new JsonData (); }, json);
        }

        public static IJsonWrapper ToWrapper (WrapperFactory factory,
                                              JsonReader reader)
        {
            return ReadValue (factory, reader);
        }

        public static IJsonWrapper ToWrapper (WrapperFactory factory,
                                              string json)
        {
            JsonReader reader = new JsonReader (json);

            return ReadValue (factory, reader);
        }

        public static void RegisterExporter<T> (ExporterFunc<T> exporter)
        {
            ExporterFunc exporter_wrapper =
                delegate (object obj, JsonWriter writer) {
                    exporter ((T) obj, writer);
                };

            custom_exporters_table[typeof (T)] = exporter_wrapper;
        }

        public static void RegisterImporter<TJson, TValue> (
            ImporterFunc<TJson, TValue> importer)
        {
            ImporterFunc importer_wrapper =
                delegate (object input) {
                    return importer ((TJson) input);
                };

            RegisterImporter (custom_importers_table, typeof (TJson),
                              typeof (TValue), importer_wrapper);
        }

        public static void UnregisterExporters ()
        {
            custom_exporters_table.Clear ();
        }

        public static void UnregisterImporters ()
        {
            custom_importers_table.Clear ();
        }
    }
}
