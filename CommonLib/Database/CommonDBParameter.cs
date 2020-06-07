﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intel.NsgAuto.WaferCost.Base.CommonLib.Database
{
    public class CommonDBParameter : DbParameter
    {
        public bool IsRefCursor { get; set; }
        public CommonDBParameter(string name, object value)
        {
            this.Value = value;
            this.ParameterName = name;
            if (value != null)
            {
                SettingDataType(value.GetType());
            }
        }
        public CommonDBParameter(string name, object value, Type type)
        {
            this.Value = value;
            this.ParameterName = name;
            SettingDataType(type);
        }
        public CommonDBParameter(string name, object value, Type type, ParameterDirection direction)
        {
            this.Value = value;
            this.ParameterName = name;
            this.Direction = direction;
            SettingDataType(type);
        }
        public CommonDBParameter(string name, object value, Type type, ParameterDirection direction, int size)
        {
            this.Value = value;
            this.ParameterName = name;
            this.Direction = direction;
            this.Size = size;
            SettingDataType(type);
        }


        public CommonDBParameter(string name, object value, System.Data.DbType type)
        {
            this.Value = value;
            this.ParameterName = name;
            this.DbType = type;
        }
        public CommonDBParameter(string name, DataTable value, string SqlServerTypeName)
        {
            this.Value = value;
            this.ParameterName = name;
            this.TypeName = SqlServerTypeName;
        }
        public CommonDBParameter(string name, object value, System.Data.DbType type, ParameterDirection direction)
        {
            this.Value = value;
            this.ParameterName = name;
            this.Direction = direction;
            this.DbType = type;
        }
        public CommonDBParameter(string name, object value, System.Data.DbType type, ParameterDirection direction, int size)
        {
            this.Value = value;
            this.ParameterName = name;
            this.Direction = direction;
            this.Size = size;
            this.DbType = type;
        }

        private void SettingDataType(Type type)
        {
            if (type == DBConstant.ByteArrayType)
            {
                this.DbType = System.Data.DbType.Binary;
            }
            else if (type == DBConstant.GuidType)
            {
                this.DbType = System.Data.DbType.Guid;
            }
            else if (type == DBConstant.IntType)
            {
                this.DbType = System.Data.DbType.Int32;
            }
            else if (type == DBConstant.ShortType)
            {
                this.DbType = System.Data.DbType.Int16;
            }
            else if (type == DBConstant.LongType)
            {
                this.DbType = System.Data.DbType.Int64;
            }
            else if (type == DBConstant.DateType)
            {
                this.DbType = System.Data.DbType.DateTime;
            }
            else if (type == DBConstant.DobType)
            {
                this.DbType = System.Data.DbType.Double;
            }
            else if (type == DBConstant.DecType)
            {
                this.DbType = System.Data.DbType.Decimal;
            }
            else if (type == DBConstant.ByteType)
            {
                this.DbType = System.Data.DbType.Byte;
            }
            else if (type == DBConstant.FloatType)
            {
                this.DbType = System.Data.DbType.Single;
            }
            else if (type == DBConstant.BoolType)
            {
                this.DbType = System.Data.DbType.Boolean;
            }
            else if (type == DBConstant.StringType)
            {
                this.DbType = System.Data.DbType.String;
            }
            else if (type == DBConstant.DateTimeOffsetType)
            {
                this.DbType = System.Data.DbType.DateTimeOffset;
            }
            else if (type == DBConstant.TimeSpanType)
            {
                if (this.Value != null)
                    this.Value = this.Value.ToString();
            }
        }
        public CommonDBParameter(string name, object value, bool isOutput)
        {
            this.Value = value;
            this.ParameterName = name;
            if (isOutput)
            {
                this.Direction = ParameterDirection.Output;
            }
        }
        public override System.Data.DbType DbType
        {
            get; set;
        }

        public override ParameterDirection Direction
        {
            get; set;
        }

        public override bool IsNullable
        {
            get; set;
        }

        public override string ParameterName
        {
            get; set;
        }

        public int _Size;

        public override int Size
        {
            get
            {
                if (_Size == 0 && Value != null)
                {
                    var isByteArray = Value.GetType() == DBConstant.ByteArrayType;
                    if (isByteArray)
                        _Size = -1;
                    else
                    {
                        var length = Value.ToString().Length;
                        _Size = length < 4000 ? 4000 : -1;

                    }
                }
                if (_Size == 0)
                    _Size = 4000;
                return _Size;
            }
            set
            {
                _Size = value;
            }
        }

        public override string SourceColumn
        {
            get; set;
        }

        public override bool SourceColumnNullMapping
        {
            get; set;
        }
        public string UdtTypeName
        {
            get;
            set;
        }

        public override object Value
        {
            get; set;
        }

        public Dictionary<string, object> TempDate
        {
            get; set;
        }

        public override void ResetDbType()
        {
            this.DbType = System.Data.DbType.String;
        }


        public string TypeName { get; set; }

        public override DataRowVersion SourceVersion
        {
            get; set;
        }
    }
}
