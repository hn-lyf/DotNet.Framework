using System;
using System.IO;
using System.Data;
using System.Collections;

namespace PETEST
{

    /// <summary>
    /// PeInfo 的摘要说明。
    /// zgke@sina.com
    /// </summary>
    public class PeInfo
    {
        /// <summary>
        /// 全部文件数据
        /// </summary>
        private byte[] PEFileByte;
        private bool _OpenFile = false;
        /// <summary>
        /// 获取是否正常打开文件
        /// </summary>
        public bool OpenFile { get { return _OpenFile; } }
        /// <summary>
        /// 文件读取的位置
        /// </summary>
        private long PEFileIndex = 0;


        private DosHeader _DosHeader;
        private DosStub _DosStub;
        private PEHeader _PEHeader;
        private OptionalHeader _OptionalHeader;
        private OptionalDirAttrib _OptionalDirAttrib;
        private SectionTable _SectionTable;
        private ExportDirectory _ExportDirectory;
        private ImportDirectory _ImportDirectory;
        private ResourceDirectory _ResourceDirectory;

        public PeInfo(string FileName)
        {
            _OpenFile = false;

            System.IO.FileStream File = new FileStream(FileName, System.IO.FileMode.Open);
            PEFileByte = new byte[File.Length];
            File.Read(PEFileByte, 0, PEFileByte.Length);
            File.Close();
            LoadFile();
            _OpenFile = true;

        }


        #region 读表方法
        /// <summary>
        /// 开始读取
        /// </summary>
        private void LoadFile()
        {
            LoadDosHeader();//取DOS
            LoadDosStub();
            LoadPEHeader();
            LoadOptionalHeader();
            LoadOptionalDirAttrib();
            LoadSectionTable();  //获取节表

            LoadExportDirectory();  //获取输出表
            LoadImportDirectory();  //获取输入表

            LoadResourceDirectory();
        }

        /// <summary>
        /// 获得DOS头
        /// </summary>
        private void LoadDosHeader()
        {
            _DosHeader = new DosHeader();

            _DosHeader.FileStarIndex = PEFileIndex;

            Loadbyte(ref _DosHeader.e_magic);
            Loadbyte(ref _DosHeader.e_cblp);
            Loadbyte(ref _DosHeader.e_cp);
            Loadbyte(ref _DosHeader.e_crlc);
            Loadbyte(ref _DosHeader.e_cparhdr);
            Loadbyte(ref _DosHeader.e_minalloc);
            Loadbyte(ref _DosHeader.e_maxalloc);
            Loadbyte(ref _DosHeader.e_ss);
            Loadbyte(ref _DosHeader.e_sp);
            Loadbyte(ref _DosHeader.e_csum);
            Loadbyte(ref _DosHeader.e_ip);
            Loadbyte(ref _DosHeader.e_cs);
            Loadbyte(ref _DosHeader.e_rva);
            Loadbyte(ref _DosHeader.e_fg);
            Loadbyte(ref _DosHeader.e_bl1);
            Loadbyte(ref _DosHeader.e_oemid);
            Loadbyte(ref _DosHeader.e_oeminfo);
            Loadbyte(ref _DosHeader.e_bl2);
            Loadbyte(ref _DosHeader.e_PESTAR);

            _DosHeader.FileEndIndex = PEFileIndex;
        }

        /// <summary>
        /// 获得DOS SUB字段
        /// </summary>
        private void LoadDosStub()
        {
            long Size = GetLong(_DosHeader.e_PESTAR) - PEFileIndex;   //获得SUB的大小
            _DosStub = new DosStub(Size);

            _DosStub.FileStarIndex = PEFileIndex;
            Loadbyte(ref _DosStub.DosStubData);
            _DosStub.FileEndIndex = PEFileIndex;
        }

        /// <summary>
        /// 获得PE的文件头
        /// </summary>
        /// <param name="Fileindex"></param>
        /// <returns></returns>
        private void LoadPEHeader()
        {
            _PEHeader = new PEHeader();
            _PEHeader.FileStarIndex = PEFileIndex;
            Loadbyte(ref _PEHeader.Header);
            Loadbyte(ref _PEHeader.Machine);
            Loadbyte(ref _PEHeader.NumberOfSections);
            Loadbyte(ref _PEHeader.TimeDateStamp);
            Loadbyte(ref _PEHeader.PointerToSymbolTable);
            Loadbyte(ref _PEHeader.NumberOfSymbols);
            Loadbyte(ref _PEHeader.SizeOfOptionalHeader);
            Loadbyte(ref _PEHeader.Characteristics);
            _PEHeader.FileEndIndex = PEFileIndex;
        }

        /// <summary>
        /// 获得OPTIONAL PE扩展属性
        /// </summary>
        /// <param name="Fileindex"></param>
        /// <returns></returns>
        private void LoadOptionalHeader()
        {
            _OptionalHeader = new OptionalHeader();

            _OptionalHeader.FileStarIndex = PEFileIndex;

            Loadbyte(ref _OptionalHeader.Magic);
            Loadbyte(ref _OptionalHeader.MajorLinkerVersion);
            Loadbyte(ref _OptionalHeader.MinorLinkerVersion);
            Loadbyte(ref _OptionalHeader.SizeOfCode);
            Loadbyte(ref _OptionalHeader.SizeOfInitializedData);
            Loadbyte(ref _OptionalHeader.SizeOfUninitializedData);
            Loadbyte(ref _OptionalHeader.AddressOfEntryPoint);
            Loadbyte(ref _OptionalHeader.BaseOfCode);
            Loadbyte(ref _OptionalHeader.ImageBase);
            Loadbyte(ref _OptionalHeader.ImageFileCode);
            Loadbyte(ref _OptionalHeader.SectionAlign);
            Loadbyte(ref _OptionalHeader.FileAlign);

            Loadbyte(ref _OptionalHeader.MajorOSV);
            Loadbyte(ref _OptionalHeader.MinorOSV);
            Loadbyte(ref _OptionalHeader.MajorImageVer);
            Loadbyte(ref _OptionalHeader.MinorImageVer);
            Loadbyte(ref _OptionalHeader.MajorSV);
            Loadbyte(ref _OptionalHeader.MinorSV);
            Loadbyte(ref _OptionalHeader.UNKNOW);
            Loadbyte(ref _OptionalHeader.SizeOfImage);
            Loadbyte(ref _OptionalHeader.SizeOfHeards);
            Loadbyte(ref _OptionalHeader.CheckSum);
            Loadbyte(ref _OptionalHeader.Subsystem);
            Loadbyte(ref _OptionalHeader.DLL_Characteristics);
            Loadbyte(ref _OptionalHeader.Bsize);
            Loadbyte(ref _OptionalHeader.TimeBsize);
            Loadbyte(ref _OptionalHeader.AucBsize);
            Loadbyte(ref _OptionalHeader.SizeOfBsize);
            Loadbyte(ref _OptionalHeader.FuckBsize);
            Loadbyte(ref _OptionalHeader.DirectCount);

            _OptionalHeader.FileEndIndex = PEFileIndex;
        }

        /// <summary>
        /// 获取目录表
        /// </summary>
        /// <param name="Fileindex"></param>
        /// <returns></returns>
        private void LoadOptionalDirAttrib()
        {
            _OptionalDirAttrib = new OptionalDirAttrib();

            _OptionalDirAttrib.FileStarIndex = PEFileIndex;

            long DirCount = GetLong(_OptionalHeader.DirectCount);

            for (int i = 0; i != DirCount; i++)
            {
                OptionalDirAttrib.DirAttrib DirectAttrib = new OptionalDirAttrib.DirAttrib();

                Loadbyte(ref DirectAttrib.DirRva);
                Loadbyte(ref DirectAttrib.DirSize);

                _OptionalDirAttrib.DirByte.Add(DirectAttrib);
            }
            _OptionalDirAttrib.FileEndIndex = PEFileIndex;
        }

        /// <summary>
        /// 获取节表
        /// </summary>
        private void LoadSectionTable()
        {
            _SectionTable = new SectionTable();
            long Count = GetLong(_PEHeader.NumberOfSections);
            _SectionTable.FileStarIndex = PEFileIndex;
            for (long i = 0; i != Count; i++)
            {
                SectionTable.SectionData Section = new SectionTable.SectionData();

                Loadbyte(ref Section.SectName);
                Loadbyte(ref Section.VirtualAddress);
                Loadbyte(ref Section.SizeOfRawDataRVA);
                Loadbyte(ref Section.SizeOfRawDataSize);
                Loadbyte(ref Section.PointerToRawData);
                Loadbyte(ref Section.PointerToRelocations);
                Loadbyte(ref Section.PointerToLinenumbers);
                Loadbyte(ref Section.NumberOfRelocations);
                Loadbyte(ref Section.NumberOfLinenumbers);
                Loadbyte(ref Section.Characteristics);
                _SectionTable.Section.Add(Section);

            }
            _SectionTable.FileEndIndex = PEFileIndex;
        }

        /// <summary>
        /// 读取输出表
        /// </summary>
        private void LoadExportDirectory()
        {

            if (_OptionalDirAttrib.DirByte.Count == 0) return;
            OptionalDirAttrib.DirAttrib ExporRVA = (OptionalDirAttrib.DirAttrib)_OptionalDirAttrib.DirByte[0];
            if (GetLong(ExporRVA.DirRva) == 0) return;


            long ExporAddress = GetLong(ExporRVA.DirRva);  //获取的位置

            _ExportDirectory = new ExportDirectory();
            for (int i = 0; i != _SectionTable.Section.Count; i++) //循环节表
            {
                SectionTable.SectionData Sect = (SectionTable.SectionData)_SectionTable.Section[i];

                long StarRva = GetLong(Sect.SizeOfRawDataRVA);
                long EndRva = GetLong(Sect.SizeOfRawDataSize);

                if (ExporAddress >= StarRva && ExporAddress < StarRva + EndRva)
                {
                    PEFileIndex = ExporAddress - GetLong(Sect.SizeOfRawDataRVA) + GetLong(Sect.PointerToRawData);

                    _ExportDirectory.FileStarIndex = PEFileIndex;
                    _ExportDirectory.FileEndIndex = PEFileIndex + GetLong(ExporRVA.DirSize);

                    Loadbyte(ref _ExportDirectory.Characteristics);
                    Loadbyte(ref _ExportDirectory.TimeDateStamp);
                    Loadbyte(ref _ExportDirectory.MajorVersion);
                    Loadbyte(ref _ExportDirectory.MinorVersion);
                    Loadbyte(ref _ExportDirectory.Name);
                    Loadbyte(ref _ExportDirectory.Base);
                    Loadbyte(ref _ExportDirectory.NumberOfFunctions);
                    Loadbyte(ref _ExportDirectory.NumberOfNames);
                    Loadbyte(ref _ExportDirectory.AddressOfFunctions);
                    Loadbyte(ref _ExportDirectory.AddressOfNames);
                    Loadbyte(ref _ExportDirectory.AddressOfNameOrdinals);

                    PEFileIndex = GetLong(_ExportDirectory.AddressOfFunctions) - GetLong(Sect.SizeOfRawDataRVA) + GetLong(Sect.PointerToRawData);
                    long EndIndex = GetLong(_ExportDirectory.AddressOfNames) - GetLong(Sect.SizeOfRawDataRVA) + GetLong(Sect.PointerToRawData);
                    long Numb = (EndIndex - PEFileIndex) / 4;
                    for (long z = 0; z != Numb; z++)
                    {
                        byte[] Data = new byte[4];
                        Loadbyte(ref Data);
                        _ExportDirectory.AddressOfFunctionsList.Add(Data);
                    }

                    Numb = 0;
                    PEFileIndex = EndIndex;
                    EndIndex = GetLong(_ExportDirectory.AddressOfNameOrdinals) - GetLong(Sect.SizeOfRawDataRVA) + GetLong(Sect.PointerToRawData);
                    Numb = (EndIndex - PEFileIndex) / 4;
                    for (long z = 0; z != Numb; z++)
                    {
                        byte[] Data = new byte[4];
                        Loadbyte(ref Data);
                        _ExportDirectory.AddressOfNamesList.Add(Data);
                    }

                    Numb = 0;
                    PEFileIndex = EndIndex;
                    EndIndex = GetLong(_ExportDirectory.Name) - GetLong(Sect.SizeOfRawDataRVA) + GetLong(Sect.PointerToRawData);
                    Numb = (EndIndex - PEFileIndex) / 2;
                    for (long z = 0; z != Numb; z++)
                    {
                        byte[] Data = new byte[2];
                        Loadbyte(ref Data);
                        _ExportDirectory.AddressOfNameOrdinalsList.Add(Data);
                    }


                    PEFileIndex = EndIndex;

                    long ReadIndex = 0;
                    while (true)
                    {
                        if (PEFileByte[PEFileIndex + ReadIndex] == 0)
                        {
                            if (PEFileByte[PEFileIndex + ReadIndex + 1] == 0) break;

                            byte[] Date = new byte[ReadIndex];
                            Loadbyte(ref Date);
                            _ExportDirectory.NameList.Add(Date);

                            PEFileIndex++;
                            ReadIndex = 0;
                        }
                        ReadIndex++;
                    }
                    break;
                }

            }

        }

        /// <summary>
        /// 读取输入表
        /// </summary>
        private void LoadImportDirectory()
        {

            if (_OptionalDirAttrib.DirByte.Count < 1) return;
            OptionalDirAttrib.DirAttrib ImporRVA = (OptionalDirAttrib.DirAttrib)_OptionalDirAttrib.DirByte[1];


            long ImporAddress = GetLong(ImporRVA.DirRva);  //获取的位置
            if (ImporAddress == 0) return;
            long ImporSize = GetLong(ImporRVA.DirSize);  //获取大小

            _ImportDirectory = new ImportDirectory();

            long SizeRva = 0;
            long PointerRva = 0;

            long StarRva = 0;
            long EndRva = 0;

            #region 获取位置
            for (int i = 0; i != _SectionTable.Section.Count; i++) //循环节表
            {
                SectionTable.SectionData Sect = (SectionTable.SectionData)_SectionTable.Section[i];

                StarRva = GetLong(Sect.SizeOfRawDataRVA);
                EndRva = GetLong(Sect.SizeOfRawDataSize);

                if (ImporAddress >= StarRva && ImporAddress < StarRva + EndRva)
                {
                    SizeRva = GetLong(Sect.SizeOfRawDataRVA);
                    PointerRva = GetLong(Sect.PointerToRawData);
                    PEFileIndex = ImporAddress - SizeRva + PointerRva;

                    _ImportDirectory.FileStarIndex = PEFileIndex;
                    _ImportDirectory.FileEndIndex = PEFileIndex + ImporSize;


                    break;
                }

            }

            if (SizeRva == 0 && PointerRva == 0) return;
            #endregion


            #region 输入表结构
            while (true)
            {

                ImportDirectory.ImportDate Import = new PeInfo.ImportDirectory.ImportDate();

                Loadbyte(ref Import.OriginalFirstThunk);
                Loadbyte(ref Import.TimeDateStamp);
                Loadbyte(ref Import.ForwarderChain);
                Loadbyte(ref Import.Name);
                Loadbyte(ref Import.FirstThunk);

                if (GetLong(Import.Name) == 0) break;

                _ImportDirectory.ImportList.Add(Import); //添加
            }
            #endregion


            #region 获取输入DLL名称
            for (int z = 0; z != _ImportDirectory.ImportList.Count; z++)     //获取引入DLL名字
            {
                ImportDirectory.ImportDate Import = (ImportDirectory.ImportDate)_ImportDirectory.ImportList[z];

                long ImportDLLName = GetLong(Import.Name) - SizeRva + PointerRva;
                PEFileIndex = ImportDLLName;
                long ReadCount = 0;
                while (true) //获取引入名
                {
                    if (PEFileByte[PEFileIndex + ReadCount] == 0)
                    {
                        Import.DLLName = new byte[ReadCount];
                        Loadbyte(ref Import.DLLName);

                        break;
                    }
                    ReadCount++;
                }
            }
            #endregion


            #region 获取引入方法 先获取地址 然后获取名字和头
            for (int z = 0; z != _ImportDirectory.ImportList.Count; z++)     //获取引入方法
            {
                ImportDirectory.ImportDate Import = (ImportDirectory.ImportDate)_ImportDirectory.ImportList[z];
                long ImportDLLName = GetLong(Import.OriginalFirstThunk) - SizeRva + PointerRva;


                PEFileIndex = ImportDLLName;
                while (true)
                {

                    ImportDirectory.ImportDate.FunctionList Function = new PeInfo.ImportDirectory.ImportDate.FunctionList();
                    Loadbyte(ref Function.OriginalFirst);

                    long LoadIndex = GetLong(Function.OriginalFirst);
                    if (LoadIndex == 0) break;
                    long OldIndex = PEFileIndex;

                    PEFileIndex = LoadIndex - SizeRva + PointerRva;

                    if (LoadIndex >= StarRva && LoadIndex < StarRva + EndRva)  //发现有些数字超级大
                    {
                        int ReadCount = 0;

                        while (true)
                        {
                            if (ReadCount == 0) Loadbyte(ref Function.FunctionHead);
                            if (PEFileByte[PEFileIndex + ReadCount] == 0)
                            {
                                byte[] FunctionName = new byte[ReadCount];
                                Loadbyte(ref FunctionName);
                                Function.FunctionName = FunctionName;

                                break;
                            }
                            ReadCount++;
                        }
                    }
                    else
                    {
                        Function.FunctionName = new byte[1];
                    }




                    PEFileIndex = OldIndex;

                    Import.DllFunctionList.Add(Function);
                }
            }
            #endregion



        }

        /// <summary>
        /// 读取资源表
        /// </summary>
        private void LoadResourceDirectory()
        {
            #region 初始化
            if (_OptionalDirAttrib.DirByte.Count < 3) return;
            OptionalDirAttrib.DirAttrib ImporRVA = (OptionalDirAttrib.DirAttrib)_OptionalDirAttrib.DirByte[2];

            long ImporAddress = GetLong(ImporRVA.DirRva);  //获取的位置
            if (ImporAddress == 0) return;
            long ImporSize = GetLong(ImporRVA.DirSize);  //获取大小


            _ResourceDirectory = new ResourceDirectory();

            long SizeRva = 0;
            long PointerRva = 0;

            long StarRva = 0;
            long EndRva = 0;
            long PEIndex = 0;
            #endregion

            #region 获取位置
            for (int i = 0; i != _SectionTable.Section.Count; i++) //循环节表
            {
                SectionTable.SectionData Sect = (SectionTable.SectionData)_SectionTable.Section[i];

                StarRva = GetLong(Sect.SizeOfRawDataRVA);
                EndRva = GetLong(Sect.SizeOfRawDataSize);

                if (ImporAddress >= StarRva && ImporAddress < StarRva + EndRva)
                {
                    SizeRva = GetLong(Sect.SizeOfRawDataRVA);
                    PointerRva = GetLong(Sect.PointerToRawData);
                    PEFileIndex = ImporAddress - SizeRva + PointerRva;
                    PEIndex = PEFileIndex;


                    _ResourceDirectory.FileStarIndex = PEFileIndex;
                    _ResourceDirectory.FileEndIndex = PEFileIndex + ImporSize;


                    break;
                }

            }





            if (SizeRva == 0 && PointerRva == 0) return;
            #endregion

            AddResourceNode(_ResourceDirectory, PEIndex, 0, StarRva);
        }

        private void AddResourceNode(ResourceDirectory Node, long PEIndex, long RVA, long ResourSectRva)
        {
            PEFileIndex = PEIndex + RVA;          //设置位置
            Loadbyte(ref Node.Characteristics);
            Loadbyte(ref Node.TimeDateStamp);
            Loadbyte(ref Node.MajorVersion);
            Loadbyte(ref Node.MinorVersion);
            Loadbyte(ref Node.NumberOfNamedEntries);
            Loadbyte(ref Node.NumberOfIdEntries);

            long NameRVA = GetLong(Node.NumberOfNamedEntries);
            for (int i = 0; i != NameRVA; i++)
            {
                ResourceDirectory.DirectoryEntry Entry = new ResourceDirectory.DirectoryEntry();
                Loadbyte(ref Entry.Name);
                Loadbyte(ref Entry.Id);
                byte[] Temp = new byte[2];
                Temp[0] = Entry.Name[0];
                Temp[1] = Entry.Name[1];

                long NameIndex = GetLong(Temp) + PEIndex;
                Temp[0] = PEFileByte[NameIndex + 0];
                Temp[1] = PEFileByte[NameIndex + 1];

                long NameCount = GetLong(Temp);
                Node.Name = new byte[NameCount * 2];

                for (int z = 0; z != Node.Name.Length; z++)
                {
                    Node.Name[z] = PEFileByte[NameIndex + 2 + z];
                }

                //System.Windows.Forms.MessageBox.Show(GetString(Entry.ID));


                Temp[0] = Entry.Id[2];
                Temp[1] = Entry.Id[3];

                long OldIndex = PEFileIndex;

                if (GetLong(Temp) == 0)
                {
                    Temp[0] = Entry.Id[0];
                    Temp[1] = Entry.Id[1];

                    PEFileIndex = GetLong(Temp) + PEIndex;

                    ResourceDirectory.DirectoryEntry.DataEntry DataRVA = new ResourceDirectory.DirectoryEntry.DataEntry();

                    Loadbyte(ref DataRVA.ResourRVA);
                    Loadbyte(ref DataRVA.ResourSize);
                    Loadbyte(ref DataRVA.ResourTest);
                    Loadbyte(ref DataRVA.ResourWen);

                    PEFileIndex = OldIndex;
                    Entry.DataEntryList.Add(DataRVA);

                    //System.Windows.Forms.MessageBox.Show(GetString(DataRVA.ResourRVA)+"*"+GetString(DataRVA.ResourSize));

                }
                else
                {
                    Temp[0] = Entry.Id[0];
                    Temp[1] = Entry.Id[1];


                    ResourceDirectory Resource = new ResourceDirectory();
                    Entry.NodeDirectoryList.Add(Resource);
                    AddResourceNode(Resource, PEIndex, GetLong(Temp), ResourSectRva);
                }

                PEFileIndex = OldIndex;

                Node.EntryList.Add(Entry);

            }

            long Count = GetLong(Node.NumberOfIdEntries);
            for (int i = 0; i != Count; i++)
            {
                ResourceDirectory.DirectoryEntry Entry = new ResourceDirectory.DirectoryEntry();
                Loadbyte(ref Entry.Name);
                Loadbyte(ref Entry.Id);

                //System.Windows.Forms.MessageBox.Show(GetString(Entry.Name)+"_"+GetString(Entry.Id));

                byte[] Temp = new byte[2];
                Temp[0] = Entry.Id[2];
                Temp[1] = Entry.Id[3];

                long OldIndex = PEFileIndex;

                if (GetLong(Temp) == 0)
                {
                    Temp[0] = Entry.Id[0];
                    Temp[1] = Entry.Id[1];

                    PEFileIndex = GetLong(Temp) + PEIndex;

                    ResourceDirectory.DirectoryEntry.DataEntry DataRVA = new ResourceDirectory.DirectoryEntry.DataEntry();

                    Loadbyte(ref DataRVA.ResourRVA);
                    Loadbyte(ref DataRVA.ResourSize);
                    Loadbyte(ref DataRVA.ResourTest);
                    Loadbyte(ref DataRVA.ResourWen);

                    long FileRva = GetLong(DataRVA.ResourRVA) - ResourSectRva + PEIndex;

                    DataRVA.FileStarIndex = FileRva;
                    DataRVA.FileEndIndex = FileRva + GetLong(DataRVA.ResourSize);



                    PEFileIndex = OldIndex;
                    Entry.DataEntryList.Add(DataRVA);

                    //System.Windows.Forms.MessageBox.Show(GetString(DataRVA.ResourRVA)+"*"+GetString(DataRVA.ResourSize));

                }
                else
                {
                    Temp[0] = Entry.Id[0];
                    Temp[1] = Entry.Id[1];


                    ResourceDirectory Resource = new ResourceDirectory();
                    Entry.NodeDirectoryList.Add(Resource);
                    AddResourceNode(Resource, PEIndex, GetLong(Temp), ResourSectRva);
                }

                PEFileIndex = OldIndex;


                Node.EntryList.Add(Entry);
            }

        }

        #endregion

        #region 类
        /// <summary>
        /// DOS文件都MS开始
        /// </summary>
        private class DosHeader
        {
            public byte[] e_magic = new byte[2]; // 魔术数字
            public byte[] e_cblp = new byte[2];  // 文件最后页的字节数
            public byte[] e_cp = new byte[2];    // 文件页数
            public byte[] e_crlc = new byte[2]; // 重定义元素个数
            public byte[] e_cparhdr = new byte[2]; // 头部尺寸，以段落为单位
            public byte[] e_minalloc = new byte[2]; // 所需的最小附加段
            public byte[] e_maxalloc = new byte[2]; // 所需的最大附加段
            public byte[] e_ss = new byte[2]; // 初始的SS值（相对偏移量）
            public byte[] e_sp = new byte[2]; // 初始的SP值
            public byte[] e_csum = new byte[2]; // 校验和
            public byte[] e_ip = new byte[2]; // 初始的IP值
            public byte[] e_cs = new byte[2]; // 初始的CS值（相对偏移量）
            public byte[] e_rva = new byte[2];
            public byte[] e_fg = new byte[2];
            public byte[] e_bl1 = new byte[8];
            public byte[] e_oemid = new byte[2];
            public byte[] e_oeminfo = new byte[2];
            public byte[] e_bl2 = new byte[20];
            public byte[] e_PESTAR = new byte[2]; //PE开始 +自己的位置 


            public long FileStarIndex = 0;
            public long FileEndIndex = 0;
        }

        /// <summary>
        /// DOS程序 提示
        /// </summary>
        private class DosStub
        {
            public byte[] DosStubData;
            public DosStub(long Size)
            {
                DosStubData = new byte[Size];
            }

            public long FileStarIndex = 0;
            public long FileEndIndex = 0;
        }

        /// <summary>
        /// PE文件头
        /// </summary>
        private class PEHeader
        {
            public byte[] Header = new byte[4];  //PE文件标记
            public byte[] Machine = new byte[2];//该文件运行所要求的CPU。对于Intel平台，该值是IMAGE_FILE_MACHINE_I386 (14Ch)。我们尝试了LUEVELSMEYER的pe.txt声明的14Dh和14Eh，但Windows不能正确执行。看起来，除了禁止程序执行之外，本域对我们来说用处不大。
            public byte[] NumberOfSections = new byte[2];//文件的节数目。如果我们要在文件中增加或删除一个节，就需要修改这个值。
            public byte[] TimeDateStamp = new byte[4];//文件创建日期和时间。我们不感兴趣。
            public byte[] PointerToSymbolTable = new byte[4];//用于调试。
            public byte[] NumberOfSymbols = new byte[4];//用于调试。
            public byte[] SizeOfOptionalHeader = new byte[2];//指示紧随本结构之后的 OptionalHeader 结构大小，必须为有效值。
            public byte[] Characteristics = new byte[2];//关于文件信息的标记，比如文件是exe还是dll。

            public long FileStarIndex = 0;
            public long FileEndIndex = 0;
        }
        /// <summary>
        /// Optinal
        /// </summary>
        private class OptionalHeader
        {
            public byte[] Magic = new byte[2]; //Magic 010B=普通可以执行，0107=ROM映像
            public byte[] MajorLinkerVersion = new byte[1]; //主版本号
            public byte[] MinorLinkerVersion = new byte[1]; //副版本号
            public byte[] SizeOfCode = new byte[4]; //代码段大小
            public byte[] SizeOfInitializedData = new byte[4]; //已初始化数据大小
            public byte[] SizeOfUninitializedData = new byte[4]; //未初始化数据大小
            public byte[] AddressOfEntryPoint = new byte[4]; //执行将从这里开始（RVA）
            public byte[] BaseOfCode = new byte[4]; //代码基址（RVA）
            public byte[] ImageBase = new byte[4]; //数据基址（RVA）
            public byte[] ImageFileCode = new byte[4]; //映象文件基址
            public byte[] SectionAlign = new byte[4]; //区段列队
            public byte[] FileAlign = new byte[4]; //文件列队

            public byte[] MajorOSV = new byte[2]; //操作系统主版本号
            public byte[] MinorOSV = new byte[2]; //操作系统副版本号
            public byte[] MajorImageVer = new byte[2]; //映象文件主版本号
            public byte[] MinorImageVer = new byte[2]; //映象文件副版本号
            public byte[] MajorSV = new byte[2]; //子操作系统主版本号
            public byte[] MinorSV = new byte[2]; //子操作系统副版本号
            public byte[] UNKNOW = new byte[4]; //Win32版本值
            public byte[] SizeOfImage = new byte[4]; //映象文件大小
            public byte[] SizeOfHeards = new byte[4]; //标志头大小
            public byte[] CheckSum = new byte[4]; //文件效验
            public byte[] Subsystem = new byte[2];//子系统（映象文件）1本地 2WINDOWS-GUI 3WINDOWS-CUI 4 POSIX-CUI
            public byte[] DLL_Characteristics = new byte[2];//DLL标记
            public byte[] Bsize = new byte[4]; //保留栈的大小
            public byte[] TimeBsize = new byte[4]; //初始时指定栈大小
            public byte[] AucBsize = new byte[4]; //保留堆的大小
            public byte[] SizeOfBsize = new byte[4]; //初始时指定堆大小
            public byte[] FuckBsize = new byte[4]; //加载器标志
            public byte[] DirectCount = new byte[4]; //数据目录数

            public long FileStarIndex = 0;
            public long FileEndIndex = 0;


        }
        /// <summary>
        /// 目录结构
        /// </summary>
        private class OptionalDirAttrib
        {
            public ArrayList DirByte = new ArrayList();

            public class DirAttrib
            {
                public byte[] DirRva = new byte[4];   //地址
                public byte[] DirSize = new byte[4];  //大小
            }

            public long FileStarIndex = 0;
            public long FileEndIndex = 0;
        }
        /// <summary>
        /// 节表
        /// </summary>
        private class SectionTable
        {
            public ArrayList Section = new ArrayList();
            public class SectionData
            {
                public byte[] SectName = new byte[8];   //名字
                public byte[] VirtualAddress = new byte[4]; //虚拟内存地址
                public byte[] SizeOfRawDataRVA = new byte[4]; //RVA偏移
                public byte[] SizeOfRawDataSize = new byte[4]; //RVA大小
                public byte[] PointerToRawData = new byte[4]; //指向RAW数据
                public byte[] PointerToRelocations = new byte[4]; //指向定位号
                public byte[] PointerToLinenumbers = new byte[4]; //指向行数
                public byte[] NumberOfRelocations = new byte[2]; //定位号
                public byte[] NumberOfLinenumbers = new byte[2]; //行数号
                public byte[] Characteristics = new byte[4]; //区段标记
            }

            public long FileStarIndex = 0;
            public long FileEndIndex = 0;
        }

        /// <summary>
        /// 输出表
        /// </summary>
        private class ExportDirectory
        {


            public byte[] Characteristics = new byte[4];//一个保留字段，目前为止值为0。
            public byte[] TimeDateStamp = new byte[4];//产生的时间。
            public byte[] MajorVersion = new byte[2];//主版本号
            public byte[] MinorVersion = new byte[2];//副版本号
            public byte[] Name = new byte[4];//一个RVA，指向一个dll的名称的ascii字符串。
            public byte[] Base = new byte[4];//输出函数的起始序号。一般为1。
            public byte[] NumberOfFunctions = new byte[4];//输出函数入口地址的数组 中的元素个数。
            public byte[] NumberOfNames = new byte[4];//输出函数名的指针的数组 中的元素个数，也是输出函数名对应的序号的数组 中的元素个数。
            public byte[] AddressOfFunctions = new byte[4]; // 一个RVA，指向输出函数入口地址的数组。
            public byte[] AddressOfNames = new byte[4]; // 一个RVA，指向输出函数名的指针的数组。
            public byte[] AddressOfNameOrdinals = new byte[4]; // 一个RVA，指向输出函数名对应的序号的数组。

            public ArrayList AddressOfFunctionsList = new ArrayList();
            public ArrayList AddressOfNamesList = new ArrayList();
            public ArrayList AddressOfNameOrdinalsList = new ArrayList();
            public ArrayList NameList = new ArrayList();

            public long FileStarIndex = 0;
            public long FileEndIndex = 0;
        }

        /// <summary>
        /// 输入表
        /// </summary>
        private class ImportDirectory
        {
            public ArrayList ImportList = new ArrayList();

            public class ImportDate
            {
                public byte[] OriginalFirstThunk = new byte[4]; //这里实际上保存着一个RVA，这个RVA指向一个DWORD数组，这个数组可以叫做输入查询表。每个数组元素，或者叫一个表项，保存着一个指向函数名的RVA或者保存着一个函数的序号。    
                public byte[] TimeDateStamp = new byte[4];//当这个值为0的时候，表明还没有bind。不为0的话，表示已经bind过了。有关bind的内容后面介绍。
                public byte[] ForwarderChain = new byte[4];
                public byte[] Name = new byte[4]; //一个RVA，这个RVA指向一个ascii以空字符结束的字符串，这个字符串就是本结构对应的dll文件的名字。
                public byte[] FirstThunk = new byte[4]; //一个RVA,这个RVA指向一个DWORD数组，这个数组可以叫输入地址表。如果bind了的话，这个数组的每个元素，就是一个输入函数的入口地址。


                public byte[] DLLName;  //DLL名称
                public ArrayList DllFunctionList = new ArrayList();
                public class FunctionList
                {
                    public byte[] OriginalFirst = new byte[4];
                    public byte[] FunctionName;
                    public byte[] FunctionHead = new byte[2];
                }

            }
            public long FileStarIndex = 0;
            public long FileEndIndex = 0;
        }

        /// <summary>
        /// 资源表
        /// </summary>
        private class ResourceDirectory
        {
            public byte[] Characteristics = new byte[4];
            public byte[] TimeDateStamp = new byte[4];
            public byte[] MajorVersion = new byte[2];
            public byte[] MinorVersion = new byte[2];
            public byte[] NumberOfNamedEntries = new byte[2];
            public byte[] NumberOfIdEntries = new byte[2];

            public byte[] Name;


            public ArrayList EntryList = new ArrayList();



            public class DirectoryEntry
            {
                public byte[] Name = new byte[4];
                public byte[] Id = new byte[4];

                public ArrayList DataEntryList = new ArrayList();

                public ArrayList NodeDirectoryList = new ArrayList();


                public class DataEntry
                {
                    public byte[] ResourRVA = new byte[4];
                    public byte[] ResourSize = new byte[4];
                    public byte[] ResourTest = new byte[4];
                    public byte[] ResourWen = new byte[4];

                    public long FileStarIndex = 0;
                    public long FileEndIndex = 0;

                }
            }

            public long FileStarIndex = 0;
            public long FileEndIndex = 0;

        }



        #endregion

        #region 工具方法
        /// <summary>
        /// 读数据 读byte[]的数量 会改边PEFileIndex的值
        /// </summary>
        /// <param name="Data"></param>
        private void Loadbyte(ref byte[] Data)
        {
            for (int i = 0; i != Data.Length; i++)
            {
                Data[i] = PEFileByte[PEFileIndex];
                PEFileIndex++;
            }
        }
        /// <summary>
        /// 转换byte为字符串
        /// </summary>
        /// <param name="Data">byte[]</param>
        /// <returns>AA BB CC DD</returns>
        private string GetString(byte[] Data)
        {
            string Temp = "";
            for (int i = 0; i != Data.Length - 1; i++)
            {
                Temp += Data[i].ToString("X02") + " ";
            }
            Temp += Data[Data.Length - 1].ToString("X02");
            return Temp;
        }
        /// <summary>
        /// 转换字符为显示数据
        /// </summary>
        /// <param name="Data">byte[]</param>
        /// <param name="Type">ASCII DEFAULT UNICODE BYTE</param>
        /// <returns></returns>
        private string GetString(byte[] Data, string Type)
        {
            if (Type.Trim().ToUpper() == "ASCII") return System.Text.Encoding.ASCII.GetString(Data);
            if (Type.Trim().ToUpper() == "DEFAULT") return System.Text.Encoding.Default.GetString(Data);
            if (Type.Trim().ToUpper() == "UNICODE") return System.Text.Encoding.Unicode.GetString(Data);
            if (Type.Trim().ToUpper() == "BYTE")
            {
                string Temp = "";
                for (int i = Data.Length - 1; i != 0; i--)
                {
                    Temp += Data[i].ToString("X02") + " ";
                }
                Temp += Data[0].ToString("X02");
                return Temp;
            }
            return GetInt(Data);
        }
        /// <summary>
        /// 转换BYTE为INT
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        private string GetInt(byte[] Data)
        {
            string Temp = "";
            for (int i = 0; i != Data.Length - 1; i++)
            {
                int ByteInt = (int)Data[i];
                Temp += ByteInt.ToString() + " ";
            }
            int EndByteInt = (int)Data[Data.Length - 1];
            Temp += EndByteInt.ToString();
            return Temp;

        }
        /// <summary>
        /// 转换数据为LONG
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        private long GetLong(byte[] Data)
        {
            string MC = "";
            if (Data.Length <= 4)
            {

                for (int i = Data.Length - 1; i != -1; i--)
                {

                    MC += Data[i].ToString("X02");

                }
            }
            else
            {
                return 0;
            }
            return Convert.ToInt64(MC, 16);
        }
        /// <summary>
        /// 添加一行信息
        /// </summary>
        /// <param name="RefTable">表</param>
        /// <param name="Data">数据</param>
        /// <param name="Name">名称</param>
        /// <param name="Describe">说明</param>
        private void AddTableRow(DataTable RefTable, byte[] Data, string Name, string Describe)
        {
            RefTable.Rows.Add(new string[]{
             Name,
             Data.Length.ToString(),
             GetString(Data),
             GetLong(Data).ToString(),
             GetString(Data,"ASCII"),
             Describe


            });
        }
        #endregion

        #region Table绘制
        /// <summary>
        /// 获取PE信息 DataSet方式
        /// </summary>
        /// <returns>多个表 最后资源表 绘制成树结构TABLE </returns>
        public DataSet GetPETable()
        {
            if (_OpenFile == false) return null;

            DataSet Ds = new DataSet("PEFile");
            if (_DosHeader != null) Ds.Tables.Add(TableDosHeader());
            if (_PEHeader != null) Ds.Tables.Add(TablePEHeader());
            if (_OptionalHeader != null) Ds.Tables.Add(TableOptionalHeader());
            if (_OptionalDirAttrib != null) Ds.Tables.Add(TableOptionalDirAttrib());
            if (_SectionTable != null) Ds.Tables.Add(TableSectionData());
            if (_ExportDirectory != null)
            {
                Ds.Tables.Add(TableExportDirectory());
                Ds.Tables.Add(TableExportFunction());
            }
            if (_ImportDirectory != null)
            {
                Ds.Tables.Add(TableImportDirectory());
                Ds.Tables.Add(TableImportFunction());
            }
            if (_ResourceDirectory != null)
            {
                Ds.Tables.Add(TableResourceDirectory());

            }
            return Ds;
        }

        private DataTable TableDosHeader()
        {
            DataTable ReturnTable = new DataTable("DosHeader FileStar{" + _DosHeader.FileStarIndex.ToString() + "}FileEnd{" + _DosHeader.FileEndIndex.ToString() + "}");
            ReturnTable.Columns.Add("Name");
            ReturnTable.Columns.Add("Size");
            ReturnTable.Columns.Add("Value16");
            ReturnTable.Columns.Add("Value10");
            ReturnTable.Columns.Add("ASCII");
            ReturnTable.Columns.Add("Describe");

            AddTableRow(ReturnTable, _DosHeader.e_magic, "e_magic", "魔术数字");
            AddTableRow(ReturnTable, _DosHeader.e_cblp, "e_cblp", "文件最后页的字节数");
            AddTableRow(ReturnTable, _DosHeader.e_cp, "e_cp", "文件页数");
            AddTableRow(ReturnTable, _DosHeader.e_crlc, "e_crlc", "重定义元素个数");
            AddTableRow(ReturnTable, _DosHeader.e_cparhdr, "e_cparhdr", "头部尺寸，以段落为单位");
            AddTableRow(ReturnTable, _DosHeader.e_minalloc, "e_minalloc", "所需的最小附加段");
            AddTableRow(ReturnTable, _DosHeader.e_maxalloc, "e_maxalloc", "所需的最大附加段");
            AddTableRow(ReturnTable, _DosHeader.e_ss, "e_ss", "初始的SS值（相对偏移量）");
            AddTableRow(ReturnTable, _DosHeader.e_sp, "e_sp", "初始的SP值");
            AddTableRow(ReturnTable, _DosHeader.e_csum, "e_csum", "校验和");
            AddTableRow(ReturnTable, _DosHeader.e_ip, "e_ip", "初始的IP值");
            AddTableRow(ReturnTable, _DosHeader.e_cs, "e_cs", "初始的CS值（相对偏移量）");
            AddTableRow(ReturnTable, _DosHeader.e_rva, "e_rva", "");
            AddTableRow(ReturnTable, _DosHeader.e_fg, "e_fg", "");
            AddTableRow(ReturnTable, _DosHeader.e_bl1, "e_bl1", "");
            AddTableRow(ReturnTable, _DosHeader.e_oemid, "e_oemid", "");
            AddTableRow(ReturnTable, _DosHeader.e_oeminfo, "e_oeminfo", "");
            AddTableRow(ReturnTable, _DosHeader.e_bl2, "e_bl2", "");
            AddTableRow(ReturnTable, _DosHeader.e_PESTAR, "e_PESTAR", "PE开始 +本结构的位置");

            return ReturnTable;
        }

        private DataTable TablePEHeader()
        {
            DataTable ReturnTable = new DataTable("PeHeader FileStar{" + _PEHeader.FileStarIndex.ToString() + "}FileEnd{" + _PEHeader.FileEndIndex.ToString() + "}");
            ReturnTable.Columns.Add("Name");
            ReturnTable.Columns.Add("Size");
            ReturnTable.Columns.Add("Value16");
            ReturnTable.Columns.Add("Value10");
            ReturnTable.Columns.Add("ASCII");
            ReturnTable.Columns.Add("Describe");


            AddTableRow(ReturnTable, _PEHeader.Header, "Header", "PE文件标记");
            AddTableRow(ReturnTable, _PEHeader.Machine, "Machine", "该文件运行所要求的CPU。对于Intel平台，该值是IMAGE_FILE_MACHINE_I386 (14Ch)。我们尝试了LUEVELSMEYER的pe.txt声明的14Dh和14Eh，但Windows不能正确执行。 ");
            AddTableRow(ReturnTable, _PEHeader.NumberOfSections, "NumberOfSections", "文件的节数目。如果我们要在文件中增加或删除一个节，就需要修改这个值。");
            AddTableRow(ReturnTable, _PEHeader.TimeDateStamp, "TimeDateStamp", "文件创建日期和时间。 ");
            AddTableRow(ReturnTable, _PEHeader.PointerToSymbolTable, "PointerToSymbolTable", "用于调试。 ");
            AddTableRow(ReturnTable, _PEHeader.NumberOfSymbols, "NumberOfSymbols", "用于调试。 ");
            AddTableRow(ReturnTable, _PEHeader.SizeOfOptionalHeader, "SizeOfOptionalHeader", "指示紧随本结构之后的 OptionalHeader 结构大小，必须为有效值。");
            AddTableRow(ReturnTable, _PEHeader.Characteristics, "Characteristics", "关于文件信息的标记，比如文件是exe还是dll。");

            return ReturnTable;
        }

        private DataTable TableOptionalHeader()
        {
            DataTable ReturnTable = new DataTable("OptionalHeader FileStar{" + _OptionalHeader.FileStarIndex.ToString() + "}FileEnd{" + _OptionalHeader.FileEndIndex.ToString() + "}");
            ReturnTable.Columns.Add("Name");
            ReturnTable.Columns.Add("Size");
            ReturnTable.Columns.Add("Value16");
            ReturnTable.Columns.Add("Value10");
            ReturnTable.Columns.Add("ASCII");
            ReturnTable.Columns.Add("Describe");

            AddTableRow(ReturnTable, _OptionalHeader.Magic, "Magic", "Magic 010B=普通可以执行，0107=ROM映像");
            AddTableRow(ReturnTable, _OptionalHeader.MajorLinkerVersion, "MajorLinkerVersion", "主版本号");
            AddTableRow(ReturnTable, _OptionalHeader.MinorLinkerVersion, "MinorLinkerVersion", "副版本号");
            AddTableRow(ReturnTable, _OptionalHeader.SizeOfCode, "SizeOfCode", "代码段大小");
            AddTableRow(ReturnTable, _OptionalHeader.SizeOfInitializedData, "SizeOfInitializedData", "已初始化数据大小");
            AddTableRow(ReturnTable, _OptionalHeader.SizeOfUninitializedData, "SizeOfUninitializedData", "未初始化数据大小");
            AddTableRow(ReturnTable, _OptionalHeader.AddressOfEntryPoint, "AddressOfEntryPoint", "执行将从这里开始（RVA）");
            AddTableRow(ReturnTable, _OptionalHeader.BaseOfCode, "BaseOfCode", "代码基址（RVA）");
            AddTableRow(ReturnTable, _OptionalHeader.ImageBase, "ImageBase", "数据基址（RVA）");
            AddTableRow(ReturnTable, _OptionalHeader.ImageFileCode, "ImageFileCode", "映象文件基址");
            AddTableRow(ReturnTable, _OptionalHeader.SectionAlign, "SectionAlign", "区段列队");
            AddTableRow(ReturnTable, _OptionalHeader.MajorOSV, "MajorOSV", "文件列队");
            AddTableRow(ReturnTable, _OptionalHeader.MinorOSV, "MinorOSV", "操作系统主版本号");
            AddTableRow(ReturnTable, _OptionalHeader.MajorImageVer, "MajorImageVer", "映象文件主版本号");
            AddTableRow(ReturnTable, _OptionalHeader.MinorImageVer, "MinorImageVer", "映象文件副版本号");
            AddTableRow(ReturnTable, _OptionalHeader.MajorSV, "MajorSV", "子操作系统主版本号");
            AddTableRow(ReturnTable, _OptionalHeader.MinorSV, "MinorSV", "子操作系统副版本号");
            AddTableRow(ReturnTable, _OptionalHeader.UNKNOW, "UNKNOW", "Win32版本值");
            AddTableRow(ReturnTable, _OptionalHeader.SizeOfImage, "SizeOfImage", "映象文件大小");
            AddTableRow(ReturnTable, _OptionalHeader.SizeOfHeards, "SizeOfHeards", "标志头大小");
            AddTableRow(ReturnTable, _OptionalHeader.CheckSum, "CheckSum", "文件效验");
            AddTableRow(ReturnTable, _OptionalHeader.Subsystem, "Subsystem", "子系统（映象文件）1本地 2WINDOWS-GUI 3WINDOWS-CUI 4 POSIX-CUI");
            AddTableRow(ReturnTable, _OptionalHeader.DLL_Characteristics, "DLL_Characteristics", "DLL标记");
            AddTableRow(ReturnTable, _OptionalHeader.Bsize, "Bsize", "保留栈的大小");
            AddTableRow(ReturnTable, _OptionalHeader.TimeBsize, "TimeBsize", "初始时指定栈大小");
            AddTableRow(ReturnTable, _OptionalHeader.AucBsize, "AucBsize", "保留堆的大小");
            AddTableRow(ReturnTable, _OptionalHeader.SizeOfBsize, "SizeOfBsize", "初始时指定堆大小");
            AddTableRow(ReturnTable, _OptionalHeader.FuckBsize, "FuckBsize", "加载器标志");
            AddTableRow(ReturnTable, _OptionalHeader.DirectCount, "DirectCount", "数据目录数");

            return ReturnTable;

        }

        private DataTable TableOptionalDirAttrib()
        {
            DataTable ReturnTable = new DataTable("OptionalDirAttrib  FileStar{" + _OptionalDirAttrib.FileStarIndex.ToString() + "}FileEnd{" + _OptionalDirAttrib.FileEndIndex.ToString() + "}");
            ReturnTable.Columns.Add("Name");
            ReturnTable.Columns.Add("Size");
            ReturnTable.Columns.Add("Value16");
            ReturnTable.Columns.Add("Value10");
            ReturnTable.Columns.Add("ASCII");
            ReturnTable.Columns.Add("Describe");

            Hashtable TableName = new Hashtable();


            TableName.Add("0", "输出表");
            TableName.Add("1", "输入表");
            TableName.Add("2", "资源表");
            TableName.Add("3", "异常表");
            TableName.Add("4", "安全表");
            TableName.Add("5", "基部重定位表");
            TableName.Add("6", "调试数据");
            TableName.Add("7", "版权数据");
            TableName.Add("8", "全局PTR");
            TableName.Add("9", "TLS表");
            TableName.Add("10", "装入配置表");
            TableName.Add("11", "其他表1");
            TableName.Add("12", "其他表2");
            TableName.Add("13", "其他表3");
            TableName.Add("14", "其他表4");
            TableName.Add("15", "其他表5");



            for (int i = 0; i != _OptionalDirAttrib.DirByte.Count; i++)
            {
                OptionalDirAttrib.DirAttrib MyDirByte = (OptionalDirAttrib.DirAttrib)_OptionalDirAttrib.DirByte[i];

                string Name = "未知表";

                if (TableName[i.ToString()] != null) Name = TableName[i.ToString()].ToString();

                AddTableRow(ReturnTable, MyDirByte.DirRva, Name, "地址");
                AddTableRow(ReturnTable, MyDirByte.DirSize, "", "大小");
            }

            return ReturnTable;
        }

        private DataTable TableSectionData()
        {
            DataTable ReturnTable = new DataTable("SectionData FileStar{" + _SectionTable.FileStarIndex.ToString() + "}FileEnd{" + _SectionTable.FileEndIndex.ToString() + "}");
            ReturnTable.Columns.Add("Name");
            ReturnTable.Columns.Add("Size");
            ReturnTable.Columns.Add("Value16");
            ReturnTable.Columns.Add("Value10");
            ReturnTable.Columns.Add("ASCII");
            ReturnTable.Columns.Add("Describe");

            for (int i = 0; i != _SectionTable.Section.Count; i++)
            {
                SectionTable.SectionData SectionDate = (SectionTable.SectionData)_SectionTable.Section[i];

                AddTableRow(ReturnTable, SectionDate.SectName, "SectName", "名字");
                AddTableRow(ReturnTable, SectionDate.VirtualAddress, "VirtualAddress", "虚拟内存地址");
                AddTableRow(ReturnTable, SectionDate.SizeOfRawDataRVA, "SizeOfRawDataRVA", "RVA偏移");
                AddTableRow(ReturnTable, SectionDate.SizeOfRawDataSize, "SizeOfRawDataSize", "RVA大小");
                AddTableRow(ReturnTable, SectionDate.PointerToRawData, "PointerToRawData", "指向RAW数据");
                AddTableRow(ReturnTable, SectionDate.PointerToRelocations, "PointerToRelocations", "指向定位号");
                AddTableRow(ReturnTable, SectionDate.PointerToLinenumbers, "PointerToLinenumbers", "指向行数");
                AddTableRow(ReturnTable, SectionDate.NumberOfRelocations, "NumberOfRelocations", "定位号");
                AddTableRow(ReturnTable, SectionDate.NumberOfLinenumbers, "NumberOfLinenumbers", "行数号");
                AddTableRow(ReturnTable, SectionDate.Characteristics, "Characteristics", "区段标记");

            }



            return ReturnTable;
        }

        private DataTable TableExportDirectory()
        {
            DataTable ReturnTable = new DataTable("ExportDirectory FileStar{" + _ExportDirectory.FileStarIndex.ToString() + "}FileEnd{" + _ExportDirectory.FileEndIndex.ToString() + "}");
            ReturnTable.Columns.Add("Name");
            ReturnTable.Columns.Add("Size");
            ReturnTable.Columns.Add("Value16");
            ReturnTable.Columns.Add("Value10");
            ReturnTable.Columns.Add("ASCII");
            ReturnTable.Columns.Add("Describe");

            AddTableRow(ReturnTable, _ExportDirectory.Characteristics, "Characteristics", "一个保留字段，目前为止值为0。");
            AddTableRow(ReturnTable, _ExportDirectory.TimeDateStamp, "TimeDateStamp", "产生的时间。");
            AddTableRow(ReturnTable, _ExportDirectory.MajorVersion, "MajorVersion", "主版本号");
            AddTableRow(ReturnTable, _ExportDirectory.MinorVersion, "MinorVersion", "副版本号");
            AddTableRow(ReturnTable, _ExportDirectory.Name, "Name", "一个RVA，指向一个dll的名称的ascii字符串。");
            AddTableRow(ReturnTable, _ExportDirectory.Base, "Base", "输出函数的起始序号。一般为1。");
            AddTableRow(ReturnTable, _ExportDirectory.NumberOfFunctions, "NumberOfFunctions", "输出函数入口地址的数组 中的元素个数。");
            AddTableRow(ReturnTable, _ExportDirectory.NumberOfNames, "NumberOfNames", "输出函数名的指针的数组 中的元素个数，也是输出函数名对应的序号的数组 中的元素个数。");
            AddTableRow(ReturnTable, _ExportDirectory.AddressOfFunctions, "AddressOfFunctions", "一个RVA，指向输出函数入口地址的数组。");
            AddTableRow(ReturnTable, _ExportDirectory.AddressOfNames, "AddressOfNames", "一个RVA，指向输出函数名的指针的数组。");
            AddTableRow(ReturnTable, _ExportDirectory.AddressOfNameOrdinals, "AddressOfNameOrdinals", "一个RVA，指向输出函数名对应的序号的数组。");


            return ReturnTable;

        }
        private DataTable TableExportFunction()
        {
            DataTable ReturnTable = new DataTable("ExportFunctionList");
            ReturnTable.Columns.Add("Name");
            ReturnTable.Columns.Add("Size");
            ReturnTable.Columns.Add("Value16");
            ReturnTable.Columns.Add("Value10");
            ReturnTable.Columns.Add("ASCII");
            ReturnTable.Columns.Add("Describe");

            for (int i = 0; i != _ExportDirectory.NameList.Count; i++)
            {
                AddTableRow(ReturnTable, (byte[])_ExportDirectory.NameList[i], "Name", "_ExportDirectory.Name-Sect.SizeOfRawDataRVA+Sect.PointerToRawData");

            }

            for (int i = 0; i != _ExportDirectory.AddressOfNamesList.Count; i++)
            {
                AddTableRow(ReturnTable, (byte[])_ExportDirectory.AddressOfNamesList[i], "NamesList", "");

            }


            for (int i = 0; i != _ExportDirectory.AddressOfFunctionsList.Count; i++)
            {
                AddTableRow(ReturnTable, (byte[])_ExportDirectory.AddressOfFunctionsList[i], "Functions", "");

            }



            for (int i = 0; i != _ExportDirectory.AddressOfNameOrdinalsList.Count; i++)
            {
                AddTableRow(ReturnTable, (byte[])_ExportDirectory.AddressOfNameOrdinalsList[i], "NameOrdinals", "");

            }


            return ReturnTable;
        }
        private DataTable TableImportDirectory()
        {
            DataTable ReturnTable = new DataTable("ImportDirectory FileStar{" + _ImportDirectory.FileStarIndex.ToString() + "}FileEnd{" + _ImportDirectory.FileEndIndex.ToString() + "}");
            ReturnTable.Columns.Add("Name");
            ReturnTable.Columns.Add("Size");
            ReturnTable.Columns.Add("Value16");
            ReturnTable.Columns.Add("Value10");
            ReturnTable.Columns.Add("ASCII");
            ReturnTable.Columns.Add("Describe");

            for (int i = 0; i != _ImportDirectory.ImportList.Count; i++)
            {
                ImportDirectory.ImportDate ImportByte = (ImportDirectory.ImportDate)_ImportDirectory.ImportList[i];

                AddTableRow(ReturnTable, ImportByte.DLLName, "输入DLL名称", "**********");
                AddTableRow(ReturnTable, ImportByte.OriginalFirstThunk, "OriginalFirstThunk", "这里实际上保存着一个RVA，这个RVA指向一个DWORD数组，这个数组可以叫做输入查询表。每个数组元素，或者叫一个表项，保存着一个指向函数名的RVA或者保存着一个函数的序号。");
                AddTableRow(ReturnTable, ImportByte.TimeDateStamp, "TimeDateStamp", "当这个值为0的时候，表明还没有bind。不为0的话，表示已经bind过了。有关bind的内容后面介绍。");
                AddTableRow(ReturnTable, ImportByte.ForwarderChain, "ForwarderChain", "");
                AddTableRow(ReturnTable, ImportByte.Name, "Name", "一个RVA，这个RVA指向一个ascii以空字符结束的字符串，这个字符串就是本结构对应的dll文件的名字。");
                AddTableRow(ReturnTable, ImportByte.FirstThunk, "FirstThunk", "一个RVA,这个RVA指向一个DWORD数组，这个数组可以叫输入地址表。如果bind了的话，这个数组的每个元素，就是一个输入函数的入口地址。");
            }

            return ReturnTable;

        }
        private DataTable TableImportFunction()
        {
            DataTable ReturnTable = new DataTable("ImportFunctionList");
            ReturnTable.Columns.Add("Name");
            ReturnTable.Columns.Add("Size");
            ReturnTable.Columns.Add("Value16");
            ReturnTable.Columns.Add("Value10");
            ReturnTable.Columns.Add("ASCII");
            ReturnTable.Columns.Add("Describe");

            for (int i = 0; i != _ImportDirectory.ImportList.Count; i++)
            {
                ImportDirectory.ImportDate ImportByte = (ImportDirectory.ImportDate)_ImportDirectory.ImportList[i];
                AddTableRow(ReturnTable, ImportByte.DLLName, "DLL-Name", "**********");

                for (int z = 0; z != ImportByte.DllFunctionList.Count; z++)
                {
                    ImportDirectory.ImportDate.FunctionList Function = (ImportDirectory.ImportDate.FunctionList)ImportByte.DllFunctionList[z];

                    AddTableRow(ReturnTable, Function.FunctionName, "FunctionName", "");
                    AddTableRow(ReturnTable, Function.FunctionHead, "FunctionHead", "");
                    AddTableRow(ReturnTable, Function.OriginalFirst, "OriginalFirstThunk", "");
                }
            }
            return ReturnTable;
        }
        private DataTable TableResourceDirectory()
        {
            DataTable ReturnTable = new DataTable("ResourceDirectory FileStar{" + _ResourceDirectory.FileStarIndex.ToString() + "}FileEnd{" + _ResourceDirectory.FileEndIndex.ToString() + "}");
            ReturnTable.Columns.Add("GUID");

            ReturnTable.Columns.Add("Text");
            ReturnTable.Columns.Add("ParentID");

            AddResourceDirectoryRow(ReturnTable, _ResourceDirectory, "");


            return ReturnTable;

        }
        private void AddResourceDirectoryRow(DataTable MyTable, ResourceDirectory Node, string ParentID)
        {
            string Name = "";
            if (Node.Name != null)
            {
                Name = GetString(Node.Name, "UNICODE");
            }

            for (int i = 0; i != Node.EntryList.Count; i++)
            {
                ResourceDirectory.DirectoryEntry Entry = (ResourceDirectory.DirectoryEntry)Node.EntryList[i];
                long ID = GetLong(Entry.Name);

                string GUID = Guid.NewGuid().ToString();

                string IDNAME = "ID{" + ID + "}";
                if (Name.Length != 0) IDNAME += "Name{" + Name + "}";

                if (ParentID.Length == 0)
                {
                    switch (ID)
                    {
                        case 1:
                            IDNAME += "Type{Cursor}";
                            break;
                        case 2:
                            IDNAME += "Type{Bitmap}";
                            break;
                        case 3:
                            IDNAME += "Type{Icon}";
                            break;
                        case 4:
                            IDNAME += "Type{Cursor}";
                            break;
                        case 5:
                            IDNAME += "Type{Menu}";
                            break;
                        case 6:
                            IDNAME += "Type{Dialog}";
                            break;
                        case 7:
                            IDNAME += "Type{String Table}";
                            break;
                        case 8:
                            IDNAME += "Type{Font Directory}";
                            break;
                        case 9:
                            IDNAME += "Type{Font}";
                            break;
                        case 10:
                            IDNAME += "Type{Accelerators}";
                            break;
                        case 11:
                            IDNAME += "Type{Unformatted}";
                            break;
                        case 12:
                            IDNAME += "Type{Message Table}";
                            break;
                        case 13:
                            IDNAME += "Type{Group Cursor}";
                            break;
                        case 14:
                            IDNAME += "Type{Group Icon}";
                            break;
                        case 15:
                            IDNAME += "Type{Information}";
                            break;
                        case 16:
                            IDNAME += "Type{Version}";
                            break;
                        default:
                            IDNAME += "Type{未定义}";
                            break;
                    }
                }

                MyTable.Rows.Add(new string[] { GUID, IDNAME, ParentID });

                for (int z = 0; z != Entry.DataEntryList.Count; z++)
                {
                    ResourceDirectory.DirectoryEntry.DataEntry Data = (ResourceDirectory.DirectoryEntry.DataEntry)Entry.DataEntryList[z];

                    string Text = "Address{" + GetString(Data.ResourRVA) + "} Size{" + GetString(Data.ResourSize) + "} FileBegin{" + Data.FileStarIndex.ToString() + "-" + Data.FileEndIndex.ToString() + "}";

                    MyTable.Rows.Add(new string[] { Guid.NewGuid().ToString(), Text, GUID });
                }

                for (int z = 0; z != Entry.NodeDirectoryList.Count; z++)
                {
                    AddResourceDirectoryRow(MyTable, (ResourceDirectory)Entry.NodeDirectoryList[z], GUID);
                }

            }

        }
        #endregion
    }



}