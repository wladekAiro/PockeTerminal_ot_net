﻿using System;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using uFCoderMulti;
using System.IO;

namespace uFR_NDEF_example
{
    public partial class frmMain : Form
    {
        string assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public const int BLUETOOTH_ADDRESS_SIZE_WITH_DELIMITERS = 17;
        private const string URI_IDENTIFIER_CODE_TAG_OPEN = "<#";
        private const string URI_IDENTIFIER_CODE_TAG_CLOSE = ">";
        private readonly int URI_IDENTIFIER_CODE_TAG_LEN = URI_IDENTIFIER_CODE_TAG_OPEN.Length + URI_IDENTIFIER_CODE_TAG_CLOSE.Length;
        private UInt32 mAsciiMirrorPos = 0;
        private string mPayload;

        public frmMain()
        {
            InitializeComponent();
            AddContextMenu(txtPayload);

            for (int i = 1; i < statNDEF.Items.Count; i++)
            {
                statNDEF.Items[i].Text = "";
            }


            for (int i = 1; i < statInfo.Items.Count; i++)
            {
                statInfo.Items[i].Text = "";
            }

            for (int i = 1; i < statDevice.Items.Count; i++)
            {
                statDevice.Items[i].Text = "";
            }

            reader_close_do();
        }

        public void AddContextMenu(RichTextBox rtb)
        {
            if (rtb.ContextMenuStrip == null)
            {
                ContextMenuStrip cms = new ContextMenuStrip { ShowImageMargin = false };

                ToolStripMenuItem tsmiCopy = new ToolStripMenuItem("Copy");
                tsmiCopy.Click += (sender, e) => rtb.Copy();
                cms.Items.Add(tsmiCopy);

                ToolStripMenuItem tsmiCopyAll = new ToolStripMenuItem("Copy All");
                tsmiCopyAll.Click += (sender, e) => rtb.Focus();
                tsmiCopyAll.Click += (sender, e) => rtb.SelectAll();
                tsmiCopyAll.Click += (sender, e) => rtb.Copy();
                cms.Items.Add(tsmiCopyAll);

                rtb.ContextMenuStrip = cms;
            }
        }

        private void prn_status(DL_STATUS status, string ok_text)
        {
            string msg;
            string status_msg = status.ToString().Replace("UFR_", "").Replace('_', ' ');

            // remove from start UFR_
            // all _ to space

            if (status == DL_STATUS.UFR_OK)
            {
                msg = " OK - " + ok_text;
                statusResult.BackColor = Color.Lime;
            }
            else
            {
                //msg = " Error: " + status_msg;
                msg = " " + status_msg;

                statusResult.BackColor = Color.Red;
            }

            // prn
            statusResult.Text = msg;
        }

        private void read_dll_version()
        {
            uint dll_ver = 0;
            byte dll_major_ver;
            byte dll_minor_ver;
            ushort dll_build;
            bool tryDefaultDllPath = false;
            bool reportDllError = false;

            //-------------------------------------------------------
            // uFR DLL
#if WIN64
            string DllPath = "..\\..\\..\\lib\\windows\\x86_64"; // for x64 target
#else
            string DllPath = "..\\..\\..\\lib\\windows\\x86"; // for x86 target
#endif
            string path = Directory.GetCurrentDirectory();
            string assemblyProbeDirectory = DllPath;
            try
            {
                Directory.SetCurrentDirectory(assemblyProbeDirectory);
                dll_ver = uFCoder.GetDllVersion();
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                tryDefaultDllPath = true;
            }
            catch (System.DllNotFoundException)
            {
                tryDefaultDllPath = true;
            }
            catch (System.BadImageFormatException)
            {
                tryDefaultDllPath = true;
            }
            Directory.SetCurrentDirectory(path);
            if (tryDefaultDllPath)
            {
                try
                {
                    dll_ver = uFCoder.GetDllVersion();
                }
                catch (System.DllNotFoundException)
                {
                    reportDllError = true;
                }
                catch (System.BadImageFormatException)
                {
                    reportDllError = true;
                }
                if (reportDllError)
                {
                    MessageBox.Show("Error while importing uFCoder library.\n" +
                        "Can't find dll file or library file is corrupted", 
                        "Dll import error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                }
            }

            dll_major_ver = (byte)dll_ver;
            dll_minor_ver = (byte)(dll_ver >> 8);
            dll_build = (byte)(dll_ver >> 16);

            DevInfoDLL.Text = " DLL: " + (dll_major_ver) + "." + (dll_minor_ver) +
                  "." + (dll_build);
        }

        private DL_STATUS reader_open()
        {
            DL_STATUS status;

            uint reader_type;
            byte[] reader_sn = new byte[8];
            byte fw_major_ver;
            byte fw_minor_ver;
            byte fw_build;
            byte hw_major;
            byte hw_minor;

            //-------------------------------------------------------
            status = uFCoder.ReaderOpen();
            if (status != DL_STATUS.UFR_OK)
            {
                return status;
            }

            //-------------------------------------------------------
            unsafe
            {
                fixed (byte* f_rdsn = reader_sn)
                    status = uFCoder.GetReaderSerialDescription(f_rdsn);
            }

            unsafe
            {
                status |= uFCoder.GetReaderType(&reader_type);

                status |= uFCoder.GetReaderHardwareVersion(&hw_major, &hw_minor);

                status |= uFCoder.GetReaderFirmwareVersion(&fw_major_ver, &fw_minor_ver);

                status |= uFCoder.GetBuildNumber(&fw_build);
            }

            if (status != DL_STATUS.UFR_OK)
            {
                return status;
            }

            //-------------------------------------------------------

            DevInfoSN.Text = " SN : " + System.Text.Encoding.UTF8.GetString(reader_sn);

            DevInfoHW.Text = " HW : " + (int)hw_major + "." + hw_minor;

            DevInfoFW.Text = " FW : " + (fw_major_ver) + "." +
                                    (fw_minor_ver) + "." +
                                    (fw_build);

            return DL_STATUS.UFR_OK;
        }

        private void reader_open_do()
        {
            statusReader.Text = " CONNECTED ";

            panelReader.BackColor = Color.LimeGreen; // LIME

            if (tabControl1.Enabled == false)
            {
                tabControl1.Enabled = true;
                //BtREAD.SetFocus();
            }
        }

        private void reader_close_do()
        {
            statusReader.Text = " Not connected";

            panelReader.BackColor = Color.Red;

            DevInfoSN.Text = " SN:";
            DevInfoHW.Text = " HW:";
            DevInfoFW.Text = " FW:";

            tabControl1.Enabled = false;
        }

        private void bOpenReader_Click(object sender, EventArgs e)
        {
            DL_STATUS status;

            panelReader.BackColor = Color.DarkRed;
            statusResult.Text = "Please wait...";
            this.Update();

            status = reader_open();

            prn_status(status, "Reader Open");

            if (status == DL_STATUS.UFR_OK)
            {
                reader_open_do();
            }
            else
            {
                reader_close_do();
            }
        }

        private void bCloseReader_Click(object sender, EventArgs e)
        {
            uFCoder.ReaderClose();
            reader_close_do();
            statusResult.Text = "";
            statusResult.BackColor = Control.DefaultBackColor;
        }

        private void bCardInit_Click(object sender, EventArgs e)
        {
            DL_STATUS status;

            status = uFCoder.ndef_card_initialization();

            prn_status(status, "Initialized");

        }

        private void bCardClear_Click(object sender, EventArgs e)
        {
            DL_STATUS status;

            byte addressingmode;
            byte address;
            byte authmode;

            byte[] sectortrailer = new byte[16] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x07, 0x80, 0x69, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff };
            //byte[]      NFCKey =new byte[6]{0xd3,0xf7,0xd3,0xf7,0xd3,0xf7};
            //byte[]      MADKey =new byte[6]{0xa0,0xa1,0xa2,0xa3,0xa4,0xa5};
            byte[] DEFKey = new byte[6] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff };
            ushort data_len;
            byte[] data;

            ushort bw;

            addressingmode = 0x01;
            address = 0x00;
            authmode = 0x61;

            for (address = 0; address < 16; address++)
            {
                unsafe
                {
                    fixed (byte* fix_sectortrailer = sectortrailer)
                    fixed (byte* fix_DEFKey = DEFKey)
                        status = uFCoder.SectorTrailerWriteUnsafe_PK(addressingmode, address, fix_sectortrailer, authmode, fix_DEFKey);
                }
            }

            data_len = (ushort)getcardlen(getcardtype());
            data = new byte[data_len + 1];

            unsafe
            {
                fixed (byte* fix_data = data)
                fixed (byte* fix_DEFKey = DEFKey)
                    status = uFCoder.LinearWrite_PK(fix_data, 0, data_len, &bw, authmode, fix_DEFKey);
            }

            prn_status(status, "Card erased");

            if (status == DL_STATUS.UFR_OK)
                MessageBox.Show("Card erased !!!");
        }

        private void bEraseAllRec_Click(object sender, EventArgs e)
        {
            DL_STATUS status;

            status = uFCoder.erase_all_ndef_records(1);

            prn_status(status, "Erased All");
        }

        private void bEraseLastRec_Click(object sender, EventArgs e)
        {
            DL_STATUS status;

            status = uFCoder.erase_last_ndef_record(1);

            prn_status(status, "Erased");
        }

        private DLOGIC_CARD_TYPE getcardtype()
        {
            DL_STATUS status;
            byte cardtype_val = 0;
            DLOGIC_CARD_TYPE cardtype;

            unsafe
            {
                status = uFCoder.GetDlogicCardType(&cardtype_val);
            }

            if (status != DL_STATUS.UFR_OK)
            {
                cardtype_val = 0;
            }

            cardtype = (DLOGIC_CARD_TYPE)cardtype_val;

            return cardtype;
        }

        private int getcardlen(DLOGIC_CARD_TYPE cardtype)
        {
            int data_len;

            switch (cardtype)
            {
                case DLOGIC_CARD_TYPE.DL_MIFARE_MINI:
                    data_len = 320;
                    break;
                case DLOGIC_CARD_TYPE.DL_MIFARE_CLASSIC_1K:
                    data_len = 752;
                    break;
                case DLOGIC_CARD_TYPE.DL_MIFARE_CLASSIC_4k:
                    data_len = 3356;
                    break;
                case DLOGIC_CARD_TYPE.DL_NTAG_203:
                    data_len = 144;
                    break;
                case DLOGIC_CARD_TYPE.DL_NTAG_213:
                    data_len = 144;
                    break;
                case DLOGIC_CARD_TYPE.DL_NTAG_215:
                    data_len = 504;
                    break;
                case DLOGIC_CARD_TYPE.DL_NTAG_216:
                    data_len = 888;
                    break;
                case DLOGIC_CARD_TYPE.DL_MIFARE_ULTRALIGHT:
                    data_len = 48;
                    break;
                case DLOGIC_CARD_TYPE.DL_MIFARE_ULTRALIGHT_C:
                    data_len = 144;
                    break;
                case DLOGIC_CARD_TYPE.DL_MIFARE_ULTRALIGHT_EV1_11:
                    data_len = 48;
                    break;
                case DLOGIC_CARD_TYPE.DL_MIFARE_ULTRALIGHT_EV1_21:
                    data_len = 144;
                    break;

                case DLOGIC_CARD_TYPE.DL_UNKNOWN_ISO_14443_4:
                    data_len = 8192;
                    break;
                default:
                    data_len = 0;
                    break;

            }

            return data_len;
        }


        private void get_record_count()
        {

        }

        private byte[] SubByteArray(byte[] sourceArray, int out_len)
        {
            byte[] truncArray = new byte[out_len];
            Array.Copy(sourceArray, truncArray, truncArray.Length);
            return truncArray;
        }

        private void bReadCard_Click(object sender, EventArgs e)
        {
            DL_STATUS result = DL_STATUS.UNKNOWN_ERROR;
            //byte tlv_type;
            //uint record_length;
            //ushort bytes_read;

            byte[] type = new byte[256];
            byte[] id = new byte[256];
            byte[] payload = new byte[1000];
            byte type_length, id_length, tnf;
            byte record_nr;
            byte message_cnt, record_cnt, empty_record_cnt;
            byte[] record_cnt_array = new byte[100];
            DLOGIC_CARD_TYPE cardtype;
            string ct;
            int card_len;

            txtPayload.Clear();

            cardtype = getcardtype();
            // trim DL
            // _ to spc
            ct = String.Format("[{0:X}]", (int)cardtype);
            ct += " " + cardtype.ToString();

            card_len = getcardlen(cardtype);


            if (cardtype == DLOGIC_CARD_TYPE.DL_NO_CARD)
            {
                statusResult.Text = ct;

                return;
            }

            NdefInfoCardType.Text = ct;

            unsafe
            {
                fixed (byte* pData = record_cnt_array)
                    result = uFCoder.get_ndef_record_count(&message_cnt, &record_cnt, pData, &empty_record_cnt);
            }

            prn_status(result, "Read record count");

            if (result != DL_STATUS.UFR_OK)
            {
                statusResult.Text = "Card is not initialized";
                return;
            }

            NdefInfoMsgs.Text = "Msgs : " + message_cnt;
            NdefInfoRecs.Text = "Recs : " + record_cnt;
            NdefInfoEmpty.Text = "Empty: " + empty_record_cnt;

            // init table !
            SG1.Rows.Clear();
            txtPayload.Text = "Payload:";

            //PB1.Max = record_cnt * 10;
            //PB1.Position = 0;

            uint payload_length;

            for (record_nr = 1; record_nr < record_cnt + 1; record_nr++)
            {
                //read_payload_length = 0;
                //memset(type, 0, 256);
                //memset(id, 0, 256);
                //memset(read_payload, 0, 1000);

                unsafe
                {
                    fixed (byte* f_type = type)
                    fixed (byte* f_id = id)
                    fixed (byte* f_payload = payload)

                        result = uFCoder.read_ndef_record(1, (byte)record_nr, &tnf, f_type, &type_length, f_id, &id_length, f_payload, &payload_length);
                }


                if (result != DL_STATUS.UFR_OK)
                {
                    if (result == DL_STATUS.UFR_WRONG_NDEF_CARD_FORMAT)
                        statusResult.Text = " NDEF format error";
                    else if (result == DL_STATUS.UFR_NDEF_MESSAGE_NOT_FOUND)
                        statusResult.Text = " NDEF message not found";
                    else
                        statusResult.Text = " Error: " + result;

                    break;
                }

                string str_payload = System.Text.Encoding.UTF8.GetString(SubByteArray(payload, (int)payload_length));
                string str_type = System.Text.Encoding.UTF8.GetString(SubByteArray(type, (int)type_length));
                string str_tnf = "TNF: " + System.Convert.ToString(tnf);
                //---------------------------------------------------------------------------

                string[] row = { record_nr.ToString(), str_type.ToString(), payload_length.ToString(), str_payload };

                SG1.Rows.Add(row);

                //PB1.Position = (1 + i) * 10;

                NdefInfoTNF.Text = str_tnf;

                statusResult.Text = " Reading " + record_nr + "/" + record_cnt;

            }  //for i

            SG1.Update();
            //PB1.Position = 0;

            statusResult.Text = "Reading DONE!";

            if (message_cnt == 0 && empty_record_cnt == 0)
            {
                statusResult.Text = "Card must be formatted first!";
            }
            else
            {
                if (message_cnt == 1 && empty_record_cnt == 1)
                {
                    statusResult.Text = "NFC formatted! Ready for use...";
                }
            }

        }

        private void SG1_SelectionChanged(object sender, EventArgs e)
        {
            txtPayload.Text = SG1.CurrentRow.Cells[3].Value.ToString();
        }

        private DL_STATUS ndef_write_mirroring(int TNF, string Type, string ID, byte[] Payload, UInt32 mirror_pos)
        {
            DL_STATUS result = DL_STATUS.UNKNOWN_ERROR;
            byte card_formated;
            byte tnf = (byte)TNF;
            int use_counter_ascii_mirror = chkCounterAsciiMirror.Checked ? 1 : 0;
            int use_uid_ascii_mirror = chkUidAsciiMirror.Checked ? 1 : 0;

            byte[] type = System.Text.Encoding.UTF8.GetBytes(Type);
            byte type_length = (byte)type.Length;

            byte[] id = System.Text.Encoding.UTF8.GetBytes(ID);
            byte id_length = (byte)ID.Length;
            byte[] payload = Payload;
            uint payload_length = (uint)payload.Length;

            unsafe
            {
                fixed (byte* f_type = type)
                fixed (byte* f_id = id)
                fixed (byte* f_payload = payload)
                    result = uFCoder.write_ndef_record_mirroring(1, &tnf, f_type, &type_length, f_id, &id_length,
                        f_payload, &payload_length, &card_formated,
                        use_uid_ascii_mirror, use_counter_ascii_mirror, mirror_pos);
            }

            return result;
        }

        private DL_STATUS ndef_write(int TNF, string Type, string ID, byte[] Payload)
        {
            DL_STATUS result = DL_STATUS.UNKNOWN_ERROR;



            byte card_formated;

            byte tnf = (byte)TNF;

            byte[] type = System.Text.Encoding.UTF8.GetBytes(Type);
            byte type_length = (byte)type.Length;

            byte[] id = System.Text.Encoding.UTF8.GetBytes(ID);
            byte id_length = (byte)ID.Length;

            byte[] payload = Payload;
            uint payload_length = (uint)payload.Length;

            unsafe
            {
                fixed (byte* f_type = type)
                fixed (byte* f_id = id)
                fixed (byte* f_payload = payload)
                    result = uFCoder.write_ndef_record(1, &tnf, f_type, &type_length, f_id, &id_length, f_payload, &payload_length, &card_formated);
            }

            return result;
        }

        private DL_STATUS write_emulation_ndef(int TNF, string Type, string ID, byte[] Payload)
        {
            DL_STATUS result = DL_STATUS.UNKNOWN_ERROR;

            byte tnf = (byte)TNF;

            byte[] type = System.Text.Encoding.UTF8.GetBytes(Type);
            byte type_length = (byte)type.Length;

            byte[] id = System.Text.Encoding.UTF8.GetBytes(ID);
            byte id_length = (byte)ID.Length;

            byte[] payload = Payload;
            byte payload_length = (byte)payload.Length;

            unsafe
            {
                fixed (byte* f_type = type)
                fixed (byte* f_id = id)
                fixed (byte* f_payload = payload)
                    result = uFCoder.WriteEmulationNdef(tnf, f_type, type_length, f_id, id_length, f_payload, payload_length);
            }

            return result;
        }

        private void eSMS_TextChanged(object sender, EventArgs e)
        {
            lSMSChars.Text = "Chars: " + eSMS.Text.Length;
        }

        private void bWritePhone_Click(object sender, EventArgs e)
        {
            DL_STATUS result = DL_STATUS.UNKNOWN_ERROR;
            int tnf;
            string type;
            string id;
            byte[] payload;
            string tmp_str = "";
            byte[] tmp_payload;

            // TODO : validate phone number

            if (ePhone.Text.Length == 0)
            {
                MessageBox.Show("Phone number is mandatory!");
                ePhone.Focus();
                return;
            }

            // Tel TNF=1, Type = URI = "U", Type Length =1 , payload[0]=5

            tmp_str = ePhone.Text;
            tmp_payload = System.Text.Encoding.UTF8.GetBytes(tmp_str);

            payload = new byte[tmp_payload.Length + 1];
            Array.Copy(tmp_payload, 0, payload, 1, tmp_payload.Length);
            payload[0] = 5;

            tnf = 1;
            type = "U";
            id = "";

            result = ndef_write(tnf, type, id, payload);

            prn_status(result, "Phone Written");
        }

        private void bWrSMS_Click(object sender, EventArgs e)
        {
            DL_STATUS result = DL_STATUS.UNKNOWN_ERROR;
            int tnf;
            string type;
            string id;
            byte[] payload;
            string tmp_str = "";
            byte[] tmp_payload;

            // TODO: validate SMS

            if (eSMS.Text.Length == 0)
            {
                MessageBox.Show("Phone number is mandatory!");
                eSMS.Focus();
                return;
            }

            // SMS TNF=1, Type = URI = "U", Type Length =1 , payload[0]=0 , "sms:", "?body:"
            tmp_str = "sms: " + eSMSPhone.Text + "?body=" + eSMS.Text;

            tmp_payload = System.Text.Encoding.UTF8.GetBytes(tmp_str);
            payload = new byte[tmp_payload.Length + 1];
            Array.Copy(tmp_payload, 0, payload, 1, tmp_payload.Length);
            payload[0] = 0;

            tnf = 1;
            type = "U";
            id = "";

            result = ndef_write(tnf, type, id, payload);

            prn_status(result, "SMS Written");
        }

        private void bWrURL_Click(object sender, EventArgs e)
        {
            DL_STATUS result = DL_STATUS.UNKNOWN_ERROR;
            int tnf;
            string type;
            string id;
            byte[] payload;
            byte[] tmp_payload;

            // TODO: validate URL

            if (eURL.Text == "")
            {
                MessageBox.Show("URL field is mandatory!");
                eURL.Focus();
                return;
            }

            // URL TNF=1, Type = URI = "U", Type Length =1 , payload[0]=1

            tmp_payload = System.Text.Encoding.UTF8.GetBytes(mPayload);
            payload = new byte[tmp_payload.Length + 1];
            Array.Copy(tmp_payload, 0, payload, 1, tmp_payload.Length);
            payload[0] = (byte)cbUriIdentifierCode.SelectedIndex;

            tnf = 1;
            type = "U";
            id = "";

            if (chkUidAsciiMirror.Checked || chkCounterAsciiMirror.Checked)
            {
                result = ndef_write_mirroring(tnf, type, id, payload, mAsciiMirrorPos);
            }
            else
            {
                result = ndef_write(tnf, type, id, payload);
            }

            prn_status(result, "URL Written");
        }

        private void bWrvCard_Click(object sender, EventArgs e)
        {
            DL_STATUS result = DL_STATUS.UNKNOWN_ERROR;
            int tnf;
            string type;
            string id;
            byte[] payload;
            string tmp_str = "";
            byte[] tmp_payload;

            // TODO: validate vCard



            string DispName, LastName, FirstName, Title, Company;
            string BPhone, CPhone, PPhone, BEmail, PEmail, WebURL, SkypeName;



            DispName = eDisplay.Text.Trim();
            LastName = eLast.Text.Trim();
            FirstName = eFirst.Text.Trim();
            BPhone = eBPhone.Text.Trim();
            CPhone = eCPhone.Text.Trim();
            PPhone = ePPhone.Text.Trim();
            BEmail = eBEmail.Text.Trim();
            PEmail = ePEmail.Text.Trim();
            Title = eTitle.Text.Trim();
            Company = eCompany.Text.Trim();
            WebURL = eWeb.Text.Trim();
            SkypeName = eSkype.Text.Trim();


            if (DispName.Length == 0)
            {
                MessageBox.Show("Display Name and Last Name fields are mandatory!");
                eDisplay.Focus();
                return;
            }


            if (LastName.Length == 0)
            {
                MessageBox.Show("Display Name and Last Name fields are mandatory!");
                eLast.Focus();
                return;
            }



            //format NDEF payload for vCard 3.0


            tmp_str = "BEGIN:VCARD\r\n";
            tmp_str += "VERSION:3.0\r\n";
            tmp_str += "N:" + LastName + ";";// replace N with N;CHARSET=UTF-8;ENCODING=8BIT:
            tmp_str += FirstName + ";;;\r\n";
            tmp_str += "FN:" + DispName + "\r\n";

            if (CPhone.Length != 0)
                tmp_str += "TEL;CELL:" + CPhone + "\r\n";
            if (BPhone.Length != 0)
                tmp_str += "TEL;WORK:" + BPhone + "\r\n";
            if (PPhone.Length != 0)
                tmp_str += "TEL;HOME:" + PPhone + "\r\n";
            if (BEmail.Length != 0)
                tmp_str += "EMAIL;WORK:" + BEmail + "\r\n";
            if (PEmail.Length != 0)
                tmp_str += "EMAIL;HOME:" + PEmail + "\r\n";
            if (Title.Length != 0)
                tmp_str += "TITLE:" + Title + "\r\n";
            if (Company.Length != 0)
                tmp_str += "ORG:" + Company + "\r\n";
            if (WebURL.Length != 0)
                tmp_str += "URL:" + WebURL + "\r\n";
            if (SkypeName.Length != 0)
                tmp_str += "X-SKYPE:" + SkypeName + "\r\n";
            tmp_str += "END:VCARD";

            MessageBox.Show(tmp_str, "vCard len= " + tmp_str.Length);

            // tmp_str is prepared

            // vCard TNF=2, Type = Mime = "text/x-vCard", Type length=12, payload[0]=1 , max 255 bytes

            tmp_payload = System.Text.Encoding.UTF8.GetBytes(tmp_str);
            //payload = new byte[tmp_payload.Length + 1];
            //Array.Copy(tmp_payload, 0, payload, 1, tmp_payload.Length);
            //payload[0] = 0;
            payload = tmp_payload;

            tnf = 2;
            type = "text/x-vCard";
            id = "";

            result = ndef_write(tnf, type, id, payload);

            prn_status(result, "vCard Written");
        }

        private void bWrAAR_Click(object sender, EventArgs e)
        {
            DL_STATUS result = DL_STATUS.UNKNOWN_ERROR;
            int tnf;
            string type;
            string id;
            byte[] payload;

            if (eAAR.Text.Length == 0)
            {
                MessageBox.Show("URL field is mandatory!");
                eURL.Focus();
                return;
            }

            // URL TNF=4, Type= "android.com:pkg", Type Length= 15, payload[0]=1

            payload = System.Text.Encoding.UTF8.GetBytes(eAAR.Text);

            tnf = 4;
            type = "android.com:pkg";
            id = "";

            result = ndef_write(tnf, type, id, payload);

            prn_status(result, "AAR Written");
        }

        private void maskedTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;
            if (!Regex.IsMatch(e.KeyChar.ToString(), "^[0-9a-fA-F\r]+$"))
                e.KeyChar = '.';
        }

        private void maskedTextBox1_Enter(object sender, EventArgs e)
        {
            BeginInvoke((Action)delegate
            {
                maskedTextBox1.SelectAll();
            });
        }

        private static byte[] CnvHexStr2ByteArr(string inStr)
        {
            return Array.ConvertAll(inStr.Split('-'), s => byte.Parse(s, System.Globalization.NumberStyles.HexNumber));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DL_STATUS result = DL_STATUS.UNKNOWN_ERROR;
            int tnf = 2; // media type
            string type = "application/vnd.bluetooth.ep.oob";
            string id = "";
            byte[] payload = new byte[8];
            String hexStr = maskedTextBox1.Text.Replace(" ", "").Replace("::", "").Replace(':', '-');

            if (hexStr.Length != BLUETOOTH_ADDRESS_SIZE_WITH_DELIMITERS)
            {
                MessageBox.Show("You must enter 6 hexadecimal numbers!" /* + "\nDebug: >" + hexStr + "< " + hexStr.Length */);
                maskedTextBox1.Focus();
                return;
            }

            byte[] payload_addr_fragment = CnvHexStr2ByteArr(hexStr);

            payload[1] = 0; // ...
            payload[0] = 8; // payload length - fixed for this purpose

            for (int i = 0; i < 6; i++)
            {
                payload[2 + i] = payload_addr_fragment[5 - i]; // indexes: [2 + i] where 2 is offset
            }

            // MessageBox.Show("Debug: Written value is: " + BitConverter.ToString(payload));

            result = ndef_write(tnf, type, id, payload);
            prn_status(result, "Bluetooth address written");
        }

        private void btnStartTagEmulation_Click(object sender, EventArgs e)
        {
            DL_STATUS status;

            status = uFCoder.TagEmulationStart();
            prn_status(status, "dedicated emulation mode activated");
        }

        private void btnStartCombinedEmulationMode_Click(object sender, EventArgs e)
        {
            DL_STATUS status;

            status = uFCoder.CombinedModeEmulationStart();
            prn_status(status, "dedicated emulation mode activated");
        }

        private void btnStopTagEmulation_Click(object sender, EventArgs e)
        {
            DL_STATUS status;

            status = uFCoder.TagEmulationStop();
            prn_status(status, "emulation mode deactivated");
        }

        private void btnStorePhoneToReader_Click(object sender, EventArgs e)
        {
            DL_STATUS status = DL_STATUS.UNKNOWN_ERROR;
            int tnf;
            string type;
            string id;
            byte[] payload;
            string tmp_str = "";
            byte[] tmp_payload;

            // TODO : validate phone number

            if (ePhone.Text.Length == 0)
            {
                MessageBox.Show("Phone number is mandatory!");
                ePhone.Focus();
                return;
            }

            // Tel TNF=1, Type = URI = "U", Type Length =1 , payload[0]=5

            tmp_str = ePhone.Text;
            tmp_payload = System.Text.Encoding.UTF8.GetBytes(tmp_str);

            payload = new byte[tmp_payload.Length + 1];
            Array.Copy(tmp_payload, 0, payload, 1, tmp_payload.Length);
            payload[0] = 5;

            tnf = 1;
            type = "U";
            id = "";

            status = write_emulation_ndef(tnf, type, id, payload);
            prn_status(status, "Phone written");
        }

        private void btnStoreSmsToReader_Click(object sender, EventArgs e)
        {
            DL_STATUS status = DL_STATUS.UNKNOWN_ERROR;
            int tnf;
            string type;
            string id;
            byte[] payload;
            string tmp_str = "";
            byte[] tmp_payload;

            // TODO: validate SMS

            if (eSMS.Text.Length == 0)
            {
                MessageBox.Show("Phone number is mandatory!");
                eSMS.Focus();
                return;
            }

            // SMS TNF=1, Type = URI = "U", Type Length =1 , payload[0]=0 , "sms:", "?body:"
            tmp_str = "sms: " + eSMSPhone.Text + "?body=" + eSMS.Text;

            tmp_payload = System.Text.Encoding.UTF8.GetBytes(tmp_str);
            payload = new byte[tmp_payload.Length + 1];
            Array.Copy(tmp_payload, 0, payload, 1, tmp_payload.Length);
            payload[0] = 0;

            tnf = 1;
            type = "U";
            id = "";

            status = write_emulation_ndef(tnf, type, id, payload);

            prn_status(status, "SMS written");
        }

        private void btnStoreUrlToReader_Click(object sender, EventArgs e)
        {
            DL_STATUS status = DL_STATUS.UNKNOWN_ERROR;
            int tnf;
            string type;
            string id;
            byte[] payload;
            string tmp_str = "";
            byte[] tmp_payload;

            // TODO: validate URL

            if (eURL.Text == "")
            {
                MessageBox.Show("URL field is mandatory!");
                eURL.Focus();
                return;
            }

            // URL TNF=1, Type = URI = "U", Type Length =1 , payload[0]=1
            tmp_str = eURL.Text;

            tmp_payload = System.Text.Encoding.UTF8.GetBytes(tmp_str);
            payload = new byte[tmp_payload.Length + 1];
            Array.Copy(tmp_payload, 0, payload, 1, tmp_payload.Length);
            payload[0] = 1;

            tnf = 1;
            type = "U";
            id = "";

            dlgStoreNDEFEmulation dlg = new dlgStoreNDEFEmulation(tnf, type, id, payload);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                prn_status(dlg.Status, "URL written");
            }
            dlg.Dispose();

            //status = write_emulation_ndef(tnf, type, id, payload);
            //prn_status(status, "URL written");
        }

        private void btnStoreVCardToReader_Click(object sender, EventArgs e)
        {
            DL_STATUS status = DL_STATUS.UNKNOWN_ERROR;
            int tnf;
            string type;
            string id;
            byte[] payload;
            string tmp_str = "";
            byte[] tmp_payload;
            string DispName, LastName, FirstName, Title, Company;
            string BPhone, CPhone, PPhone, BEmail, PEmail, WebURL, SkypeName;

            // TODO: validate vCard

            DispName = eDisplay.Text.Trim();
            LastName = eLast.Text.Trim();
            FirstName = eFirst.Text.Trim();
            BPhone = eBPhone.Text.Trim();
            CPhone = eCPhone.Text.Trim();
            PPhone = ePPhone.Text.Trim();
            BEmail = eBEmail.Text.Trim();
            PEmail = ePEmail.Text.Trim();
            Title = eTitle.Text.Trim();
            Company = eCompany.Text.Trim();
            WebURL = eWeb.Text.Trim();
            SkypeName = eSkype.Text.Trim();

            if (DispName.Length == 0)
            {
                MessageBox.Show("Display Name and Last Name fields are mandatory!");
                eDisplay.Focus();
                return;
            }

            if (LastName.Length == 0)
            {
                MessageBox.Show("Display Name and Last Name fields are mandatory!");
                eLast.Focus();
                return;
            }

            //format NDEF payload for vCard 3.0
            tmp_str = "BEGIN:VCARD\r\n";
            tmp_str += "VERSION:3.0\r\n";
            tmp_str += "N:" + LastName + ";";// replace N with N;CHARSET=UTF-8;ENCODING=8BIT:
            tmp_str += FirstName + ";;;\r\n";
            tmp_str += "FN:" + DispName + "\r\n";

            if (CPhone.Length != 0)
                tmp_str += "TEL;CELL:" + CPhone + "\r\n";
            if (BPhone.Length != 0)
                tmp_str += "TEL;WORK:" + BPhone + "\r\n";
            if (PPhone.Length != 0)
                tmp_str += "TEL;HOME:" + PPhone + "\r\n";
            if (BEmail.Length != 0)
                tmp_str += "EMAIL;WORK:" + BEmail + "\r\n";
            if (PEmail.Length != 0)
                tmp_str += "EMAIL;HOME:" + PEmail + "\r\n";
            if (Title.Length != 0)
                tmp_str += "TITLE:" + Title + "\r\n";
            if (Company.Length != 0)
                tmp_str += "ORG:" + Company + "\r\n";
            if (WebURL.Length != 0)
                tmp_str += "URL:" + WebURL + "\r\n";
            if (SkypeName.Length != 0)
                tmp_str += "X-SKYPE:" + SkypeName + "\r\n";
            tmp_str += "END:VCARD";

            MessageBox.Show(tmp_str, "vCard len= " + tmp_str.Length);

            // tmp_str is prepared
            // vCard TNF=2, Type = Mime = "text/x-vCard", Type length=12, payload[0]=1 , max 255 bytes
            tmp_payload = System.Text.Encoding.UTF8.GetBytes(tmp_str);
            payload = tmp_payload;

            tnf = 2;
            type = "text/x-vCard";
            id = "";

            status = write_emulation_ndef(tnf, type, id, payload);
            prn_status(status, "vCard written");
        }

        private void btnStoreBluetoothToReader_Click(object sender, EventArgs e)
        {
            DL_STATUS status = DL_STATUS.UNKNOWN_ERROR;
            int tnf = 2; // media type
            string type = "application/vnd.bluetooth.ep.oob";
            string id = "";
            byte[] payload = new byte[8];
            String hexStr = maskedTextBox1.Text.Replace(" ", "").Replace("::", "").Replace(':', '-');

            if (hexStr.Length != BLUETOOTH_ADDRESS_SIZE_WITH_DELIMITERS)
            {
                MessageBox.Show("You must enter 6 hexadecimal numbers!" /* + "\nDebug: >" + hexStr + "< " + hexStr.Length */);
                maskedTextBox1.Focus();
                return;
            }

            byte[] payload_addr_fragment = CnvHexStr2ByteArr(hexStr);

            payload[1] = 0; // ...
            payload[0] = 8; // payload length - fixed for this purpose

            for (int i = 0; i < 6; i++)
            {
                payload[2 + i] = payload_addr_fragment[5 - i]; // indexes: [2 + i] where 2 is offset
            }

            // MessageBox.Show("Debug: Written value is: " + BitConverter.ToString(payload));

            status = write_emulation_ndef(tnf, type, id, payload);
            prn_status(status, "Bluetooth address written");
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            cbUriIdentifierCode.SelectedIndex = 1;
            this.Text = this.Text + " v" + assemblyVersion;
            read_dll_version();
        }

        private void updatePayload(object sender, EventArgs e)
        {
            int UsedUriIdentifierCodeTagLen = -1;
            tbPayload.Text = "";
            mPayload = "";

            if (cbUriIdentifierCode.SelectedIndex != 0)
            {
                UsedUriIdentifierCodeTagLen = URI_IDENTIFIER_CODE_TAG_LEN + cbUriIdentifierCode.SelectedIndex.ToString().Length - 1;
                tbPayload.Text = URI_IDENTIFIER_CODE_TAG_OPEN + cbUriIdentifierCode.SelectedIndex + URI_IDENTIFIER_CODE_TAG_CLOSE;
            }

            tbPayload.Text += eURL.Text;
            mPayload += eURL.Text;

            if (chkUidAsciiMirror.Checked && chkCounterAsciiMirror.Checked)
            {
                mAsciiMirrorPos = (UInt32)(tbPayload.Text.Length + tbAsciiMirror.Text.Length - UsedUriIdentifierCodeTagLen);
                tbPayload.Text += tbAsciiMirror.Text + "00000000000000x000000";
                mPayload += tbAsciiMirror.Text + "00000000000000x000000";
            }
            else if (chkUidAsciiMirror.Checked || chkCounterAsciiMirror.Checked)
            {
                mAsciiMirrorPos = (UInt32)(tbPayload.Text.Length + tbAsciiMirror.Text.Length - UsedUriIdentifierCodeTagLen);
                if (chkUidAsciiMirror.Checked)
                {
                    tbPayload.Text += tbAsciiMirror.Text + "00000000000000";
                    mPayload += tbAsciiMirror.Text + "00000000000000";
                }
                if (chkCounterAsciiMirror.Checked)
                {
                    tbPayload.Text += tbAsciiMirror.Text + "000000";
                    mPayload += tbAsciiMirror.Text + "000000";
                }
            }
            else
            {
                mAsciiMirrorPos = 0;
            }
        }

        private void chkUidAsciiMirror_CheckedChanged(object sender, EventArgs e)
        {
            lbAsciiMirror.Enabled = chkUidAsciiMirror.Checked || chkCounterAsciiMirror.Checked;
            tbAsciiMirror.Enabled = chkUidAsciiMirror.Checked || chkCounterAsciiMirror.Checked;

            btnStoreUrlToReader.Enabled = !chkUidAsciiMirror.Checked && !chkCounterAsciiMirror.Checked;
            updatePayload(sender, e);
        }

        private void chkCounterAsciiMirror_CheckedChanged(object sender, EventArgs e)
        {
            lbAsciiMirror.Enabled = chkUidAsciiMirror.Checked || chkCounterAsciiMirror.Checked;
            tbAsciiMirror.Enabled = chkUidAsciiMirror.Checked || chkCounterAsciiMirror.Checked;

            btnStoreUrlToReader.Enabled = !chkUidAsciiMirror.Checked && !chkCounterAsciiMirror.Checked;
            updatePayload(sender, e);
        }

        private void chkShowPasswd_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowPasswd.Checked)
            {
                tbPasswd.PasswordChar = '\0';
            }
            else
            {
                tbPasswd.PasswordChar = '*';
            }
        }

        private void btUnlock_Click(object sender, EventArgs e)
        {
            DL_STATUS status = DL_STATUS.UNKNOWN_ERROR;

            status = uFCoder.EE_Lock(tbPasswd.Text, 0);

            prn_status(status, "NV Unlocked");
        }

        private void btLock_Click(object sender, EventArgs e)
        {
            DL_STATUS status = DL_STATUS.UNKNOWN_ERROR;

            status = uFCoder.EE_Lock(tbPasswd.Text, 1);

            prn_status(status, "NV Locked");
        }
    }
}
