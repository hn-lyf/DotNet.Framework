using System;

namespace DotNet
{
    /// <summary>
    /// 标准的Snowflake算法,2010年开始  可以使用到2079年
    /// <para>默认计算规则</para>
    /// <para>第1位为0表示正数</para>
    /// <para>第2-42位为毫秒级时间戳</para>
    /// <para>第43-52位为工作机ID 最多1023台机器</para>
    /// <para>第53-64位为序列号</para>
    /// </summary>
    public class Snowflake
    {
        /// <summary>
        /// 配置信息。
        /// </summary>
        public static class Config
        {
            static Config()
            {
                var snowflakeMachineId = Configuration.GetSetting("Snowflake:MachineId");
                if (!string.IsNullOrWhiteSpace(snowflakeMachineId))
                {
                    ushort.TryParse(snowflakeMachineId, out machineId);
                }
                var snowflakeBeginTicks = Configuration.GetSetting("Snowflake:BeginTicks");
                if (!string.IsNullOrWhiteSpace(snowflakeBeginTicks))
                {
                    long.TryParse(snowflakeBeginTicks, out beginTicks);
                }
            }
            /// <summary>
            /// 10个bit   最多1024台机器
            /// </summary>
            private static ushort machineId = 0;
            /// <summary>
            /// 获取或设置工作机ID  （10位） 最大值位1023
            /// </summary>
            public static ushort MachineId
            {
                get { return machineId; }
                set
                {
                    if (value > MachineIdMask)
                    {
                        throw new NotFiniteNumberException();
                    }
                    machineId = value;
                }
            }
            /// <summary>
            /// 获取最大机器编号
            /// </summary>
            public static ushort MachineIdMask
            {
                get
                {
                    return (ushort)(-1 ^ (-1 << MachineIdBit));
                }
            }

            /// <summary>
            /// 机器ID10个bit   最多1024台机器
            /// </summary>
            private static byte machineIdBit = 10;
            /// <summary>
            /// 获取或设置机器Id的bit位。默认为10个bit位。
            /// </summary>
            public static byte MachineIdBit { get => machineIdBit; set => machineIdBit = value; }
            /// <summary>
            ///  12 位  最多4095  
            /// </summary>
            private static byte sequenceBit = 12;
            /// <summary>
            ///  获取或设置序列号的bit位。默认为12个bit位。
            /// </summary>
            public static byte SequenceBit { get => sequenceBit; set => sequenceBit = value; }
            /// <summary>
            /// 获取最大序列号
            /// </summary>
            public static long SequenceMask
            {
                get
                {
                    return -1L ^ (-1L << SequenceBit);
                }
            }
            /// <summary>
            /// 开始时间的计时周期数。
            /// </summary>
            private static long beginTicks = 633979008000000000L;
            /// <summary>
            /// 开始时间的计时周期数，默认为2010年1月1日
            /// </summary>
            public static long BeginTicks
            {
                get { return beginTicks; }
                set { beginTicks = value; }
            }
        }
        /// <summary>
        /// 加锁对象
        /// </summary>
        private static readonly object syncLook = new object();//加锁对象
        private static Snowflake instance;
        /// <summary>
        /// 获取<see cref="Snowflake"/>对象。
        /// </summary>
        public static Snowflake Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncLook)
                    {
                        if (instance == null)
                        {
                            instance = new Snowflake();
                        }
                    }
                }
                return instance;
            }
        }
        /// <summary>
        /// 最后时间戳
        /// </summary>
        private long lastTimestamp = 0;//最后时间戳
        /// <summary>
        /// 当前的序列号
        /// </summary>
        private long sequence;
        /// <summary>
        /// 开始时间的计时周期数
        /// </summary>
        public virtual long Sequence
        {
            get { return sequence; }
        }
        /// <summary>
        /// 产生一个新的Id
        /// <para>第1位为0表示正数</para>
        /// <para>第2-42位为毫秒级时间戳</para>
        /// <para>第43-52位为工作机ID</para>
        /// <para>第53-64位为序列号</para>
        /// </summary>
        /// <returns></returns>
        public static long NewId()
        {
            return Instance.CreateId();
        }

        /// <summary>
        /// 产生一个新的Timestamp
        /// </summary>
        /// <returns></returns>
        protected virtual long NewTimestamp()
        {
            long timestamp = (DateTime.Now.Ticks - Config.BeginTicks) / 10000;
            if (timestamp == lastTimestamp)
            {
                //同一微妙中生成ID
                sequence = (ushort)(++sequence & Config.SequenceMask);
                if (sequence == 0)
                {
                    System.Threading.Thread.Sleep(1);
                    return NewTimestamp();
                }
            }
            else
            {
                //不同微秒生成ID
                sequence = 1;
            }
            return timestamp;
        }
        /// <summary>
        /// 产生一个新的Id
        /// <para>第1位为0表示正数</para>
        /// <para>第2-42位为毫秒级时间戳</para>
        /// <para>第43-52位为工作机ID</para>
        /// <para>第53-64位为序列号</para>
        /// </summary>
        /// <returns></returns>
        protected virtual long CreateId()
        {
            long snowflakeId = 0;
            lock (syncLook)
            {
                lastTimestamp = NewTimestamp();
                snowflakeId = (long)(lastTimestamp << (Config.MachineIdBit + Config.SequenceBit)) | (long)(Config.MachineId << Config.SequenceBit) | sequence;
            }
            return snowflakeId;
        }
        /// <summary>
        /// 获取ID中的时间戳
        /// </summary>
        /// <param name="id"><see cref="Snowflake"/>生成的Id的编号</param>
        /// <returns></returns>
        public static DateTime GetDateTime(long id)
        {
            return new DateTime(((id >> Config.SequenceBit) >> Config.MachineIdBit) * 10000 + Config.BeginTicks);
        }
        /// <summary>
        /// 将时间换算成可与<see cref="Snowflake"/>的编号 
        /// <para>大于<see cref="Snowflake"/>的编号</para>
        /// </summary>
        /// <param name="dateTime">要转换的时间</param>
        /// <returns></returns>
        public static long DateTimeToEqualsId(DateTime dateTime)
        {
            return ((dateTime.Ticks - Config.BeginTicks) / 10000) << Config.SequenceBit << Config.MachineIdBit;
        }
    }
}
