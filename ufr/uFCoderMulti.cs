﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uFCoderMulti
{
    using System.Runtime.InteropServices;
    using UFR_HANDLE = System.UIntPtr;

    enum CARD_SAK
    {
        UNKNOWN = 0x00,
        MIFARE_CLASSIC_1k = 0x08,
        MF1ICS50 = 0x08,
        SLE66R35 = 0x88,
        MIFARE_CLASSIC_4k = 0x18,
        MF1ICS70 = 0x18,
        MIFARE_CLASSIC_MINI = 0x09,
        MF1ICS20 = 0x09,
    }

    enum DLOGIC_CARD_TYPE
    {
        DL_NO_CARD = 0x00,
        DL_MIFARE_ULTRALIGHT = 0x01,
        DL_MIFARE_ULTRALIGHT_EV1_11 = 0x02,
        DL_MIFARE_ULTRALIGHT_EV1_21 = 0x03,
        DL_MIFARE_ULTRALIGHT_C = 0x04,
        DL_NTAG_203 = 0x05,
        DL_NTAG_210 = 0x06,
        DL_NTAG_212 = 0x07,
        DL_NTAG_213 = 0x08,
        DL_NTAG_215 = 0x09,
        DL_NTAG_216 = 0x0A,
        DL_MIKRON_MIK640D = 0x0B,

        DL_MIFARE_CLASSIC_4k = 0x18,

        DL_MIFARE_MINI = 0x20,
        DL_MIFARE_CLASSIC_1K = 0x21,
        DL_MIFARE_CLASSIC_4K = 0x22,
        DL_MIFARE_PLUS_S_2K = 0x23,
        DL_MIFARE_PLUS_S_4K = 0x24,
        DL_MIFARE_PLUS_X_2K = 0x25,
        DL_MIFARE_PLUS_X_4K = 0x26,
        DL_MIFARE_DESFIRE = 0x27,
        DL_MIFARE_DESFIRE_EV1_2K = 0x28,
        DL_MIFARE_DESFIRE_EV1_4K = 0x29,
        DL_MIFARE_DESFIRE_EV1_8K = 0x2A,

        DL_UNKNOWN_ISO_14443_4 = 0x40
    }

    // MIFARE CLASSIC Authentication Modes:
    enum MIFARE_AUTHENTICATION
    {
        MIFARE_AUTHENT1A = 0x60,
        MIFARE_AUTHENT1B = 0x61,
    }

    // API Status Codes Type:
    public enum DL_STATUS
    {
        UFR_OK = 0x00,

        UFR_COMMUNICATION_ERROR = 0x01,
        UFR_CHKSUM_ERROR = 0x02,
        UFR_READING_ERROR = 0x03,
        UFR_WRITING_ERROR = 0x04,
        UFR_BUFFER_OVERFLOW = 0x05,
        UFR_MAX_ADDRESS_EXCEEDED = 0x06,
        UFR_MAX_KEY_INDEX_EXCEEDED = 0x07,
        UFR_NO_CARD = 0x08,
        UFR_COMMAND_NOT_SUPPORTED = 0x09,
        UFR_FORBIDEN_DIRECT_WRITE_IN_SECTOR_TRAILER = 0x0A,
        UFR_ADDRESSED_BLOCK_IS_NOT_SECTOR_TRAILER = 0x0B,
        UFR_WRONG_ADDRESS_MODE = 0x0C,
        UFR_WRONG_ACCESS_BITS_VALUES = 0x0D,
        UFR_AUTH_ERROR = 0x0E,
        UFR_PARAMETERS_ERROR = 0x0F, // ToDo, tačka 5.
        UFR_MAX_SIZE_EXCEEDED = 0x10,

        UFR_WRITE_VERIFICATION_ERROR = 0x70,
        UFR_BUFFER_SIZE_EXCEEDED = 0x71,
        UFR_VALUE_BLOCK_INVALID = 0x72,
        UFR_VALUE_BLOCK_ADDR_INVALID = 0x73,
        UFR_VALUE_BLOCK_MANIPULATION_ERROR = 0x74,
        UFR_WRONG_UI_MODE = 0x75,
        UFR_KEYS_LOCKED = 0x76,
        UFR_KEYS_UNLOCKED = 0x77,
        UFR_WRONG_PASSWORD = 0x78,
        UFR_CAN_NOT_LOCK_DEVICE = 0x79,
        UFR_CAN_NOT_UNLOCK_DEVICE = 0x7A,
        UFR_DEVICE_EEPROM_BUSY = 0x7B,
        UFR_RTC_SET_ERROR = 0x7C,

        UFR_COMMUNICATION_BREAK = 0x50,
        UFR_NO_MEMORY_ERROR = 0x51,
        UFR_CAN_NOT_OPEN_READER = 0x52,
        UFR_READER_NOT_SUPPORTED = 0x53,
        UFR_READER_OPENING_ERROR = 0x54,
        UFR_READER_PORT_NOT_OPENED = 0x55,
        UFR_CANT_CLOSE_READER_PORT = 0x56,

        UFR_FT_STATUS_ERROR_1 = 0xA0,
        UFR_FT_STATUS_ERROR_2 = 0xA1,
        UFR_FT_STATUS_ERROR_3 = 0xA2,
        UFR_FT_STATUS_ERROR_4 = 0xA3,
        UFR_FT_STATUS_ERROR_5 = 0xA4,
        UFR_FT_STATUS_ERROR_6 = 0xA5,
        UFR_FT_STATUS_ERROR_7 = 0xA6,
        UFR_FT_STATUS_ERROR_8 = 0xA7,
        UFR_FT_STATUS_ERROR_9 = 0xA8,

        //NDEF error codes
        UFR_WRONG_NDEF_CARD_FORMAT = 0x80,
        UFR_NDEF_MESSAGE_NOT_FOUND = 0x81,
        UFR_NDEF_UNSUPPORTED_CARD_TYPE = 0x82,
        UFR_NDEF_CARD_FORMAT_ERROR = 0x83,
        UFR_MAD_NOT_ENABLED = 0x84,
        UFR_MAD_VERSION_NOT_SUPPORTED = 0x85,

        // multi units
        UFR_DEVICE_WRONG_HANDLE = 0x100,
        UFR_DEVICE_INDEX_OUT_OF_BOUND,
        UFR_DEVICE_ALREADY_OPENED,
        UFR_DEVICE_ALREADY_CLOSED,

        MAX_UFR_STATUS = 10000000,
        UNKNOWN_ERROR = 2147483647 // 0x7FFFFFFF
    };

    public static unsafe class uFCoder
    {
        //--------------------------------------------------------------------------------------------------
#if WIN64
        public const string DLL_NAME = "uFCoder-x86_64.dll"; // for x64 target
#else
        public const string DLL_NAME = "uFCoder-x86.dll"; // for x86 target
#endif
        //--------------------------------------------------------------------------------------------------

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "ReaderOpen")]
        public static extern DL_STATUS ReaderOpen();

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "ReaderClose")]
        public static extern DL_STATUS ReaderClose();

        //--------------------------------------------------------------------------------------------------

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetReaderType")]
        public static extern DL_STATUS GetReaderType(uint* lpulReaderType);


        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetReaderSerialDescription")]
        public static extern DL_STATUS GetReaderSerialDescription(byte* pSerialDescription);

        //--------------------------------------------------------------------------------------------------

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "read_ndef_record")]
        public static extern DL_STATUS read_ndef_record(byte message_nr, byte record_nr, byte* tnf, byte* type_record, byte* type_length, byte* id, byte* id_length, byte* payload, uint* payload_length);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "write_ndef_record")]
        public static extern DL_STATUS write_ndef_record(byte message_nr, byte* tnf, byte* type_record, byte* type_length, byte* id, byte* id_length,
           byte* payload, uint* payload_length, byte* card_formated);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "write_ndef_record_mirroring")]
        public static extern DL_STATUS write_ndef_record_mirroring(byte message_nr,
                  byte* tnf, byte* type_record, byte* type_length, byte* id,
                  byte* id_length, byte* payload, UInt32* payload_length,
                  byte* card_formated, int use_uid_ascii_mirror, int use_counter_ascii_mirror, UInt32 payload_mirroring_pos);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "get_ndef_record_count")]
        public static extern DL_STATUS get_ndef_record_count(byte* ndef_message_cnt, byte* ndef_record_cnt, byte* ndef_record_array, byte* empty_ndef_message_cnt);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "erase_last_ndef_record")]
        public static extern DL_STATUS erase_last_ndef_record(byte message_nr);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "erase_all_ndef_records")]
        public static extern DL_STATUS erase_all_ndef_records(byte message_nr);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "ndef_card_initialization")]
        public static extern DL_STATUS ndef_card_initialization();

        //---------------------------------------------------------------------
        // Card emulation:
        //---------------------------------------------------------------------
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "WriteEmulationNdef")]
        public static extern DL_STATUS WriteEmulationNdef(byte tnf, byte* type_record, byte type_length, byte* id, byte id_length,
                                                            byte* payload, byte payload_length);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "WriteEmulationNdefWithAAR")]
        public static extern DL_STATUS WriteEmulationNdefWithAAR(byte tnf, byte* type_record, byte type_length, byte* id, byte id_length,
                                                                 byte* payload, byte payload_length, byte* aar, byte aar_length);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "TagEmulationStart")]
        public static extern DL_STATUS TagEmulationStart();

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "TagEmulationStop")]
        public static extern DL_STATUS TagEmulationStop();

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "CombinedModeEmulationStart")]
        public static extern DL_STATUS CombinedModeEmulationStart();
        //---------------------------------------------------------------------

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetDlogicCardType")]
        public static extern DL_STATUS GetDlogicCardType(byte* lpucCardType);

        //---------------------------------------------------------------------
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "ReaderList_UpdateAndGetCount")]
        public static extern DL_STATUS ReaderList_UpdateAndGetCount(Int32* NumberOfDevices);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "ReaderList_GetSerialByIndex")]
        public static extern DL_STATUS ReaderList_GetSerialByIndex(Int32 DeviceIndex, UInt32* lpulSerialNumber);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "ReaderList_GetSerialDesByIndex")]
        public static extern DL_STATUS ReaderList_GetSerialDescriptionByIndex(Int32 DeviceIndex, char* pSerialDescription);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "ReaderList_GetTypeByIndex")]
        public static extern DL_STATUS ReaderList_GetTypeByIndex(Int32 DeviceIndex, UInt32* lpulReaderType);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "ReaderList_GetFTDISerialByIndex")]
        public static extern DL_STATUS ReaderList_GetFTDISerialByIndex(Int32 DeviceIndex, char** Device_Serial);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "ReaderList_GetFTDIDescriptionByIndex")]
        public static extern DL_STATUS ReaderList_GetFTDIDescriptionByIndex(Int32 DeviceIndex, char** Device_Description);

        //---------------------------------------------------------------------

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "ReaderList_OpenByIndex")]
        public static extern DL_STATUS ReaderList_OpenByIndex(Int32 DeviceIndex, UFR_HANDLE* hndUFR);

        //---------------------------------------------------------------------

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "ReaderOpenM")]
        public static extern DL_STATUS ReaderOpenM(UFR_HANDLE hndUFR);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "ReaderCloseM")]
        public static extern DL_STATUS ReaderCloseM(UFR_HANDLE hndUFR);

        //---------------------------------------------------------------------
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetCardIdExM")]
        public static extern DL_STATUS GetCardIdExM(UFR_HANDLE hndUFR,
                                                    byte* bCardType,
                                                    byte* bCardUID,
                                                    byte* bUidSize);

        //[DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetReaderTypeM")]
        //public static extern DL_STATUS GetReaderType(UInt32* get_reader_type);

        //---------------------------------------------------------------------

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "BlockRead_PKM")]
        public static extern DL_STATUS BlockRead_PKM(UFR_HANDLE hndUFR,
                                                  byte* data,
                                                  byte block_address,
                                                  byte auth_mode,
                                                  byte* key);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "BlockWrite_PKM")]
        public static extern DL_STATUS BlockWrite_PKM(UFR_HANDLE hndUFR,
                                                    byte* data,
                                                  byte block_address,
                                                  byte auth_mode,
                                                  byte* key);

        //---------------------------------------------------------------------
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetDllVersion")]
        public static extern uint GetDllVersion();

        //---------------------------------------------------------------------
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetReaderHardwareVersion")]
        public static extern DL_STATUS GetReaderHardwareVersion(byte* version_major, byte* version_minor);

        //---------------------------------------------------------------------
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetReaderFirmwareVersion")]
        public static extern DL_STATUS GetReaderFirmwareVersion(byte* version_major, byte* version_minor);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetBuildNumber")]
        public static extern DL_STATUS GetBuildNumber(byte* build);

        //---------------------------------------------------------------------
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "SectorTrailerWriteUnsafe_PK")]
        public static extern DL_STATUS SectorTrailerWriteUnsafe_PK(byte addressing_mode, byte address, byte* sector_trailer,
                                                  byte auth_mode, byte* key);
        //---------------------------------------------------------------------
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "LinearWrite_PK")]
        public static extern DL_STATUS LinearWrite_PK(byte* data, ushort linear_address, ushort length, ushort* bytes_written,
                                     byte auth_mode, byte* key);
        //---------------------------------------------------------------------
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "EE_Lock")]
        private static extern DL_STATUS Linkage_EE_Lock(StringBuilder password, UInt32 locked);
        public static DL_STATUS EE_Lock(String password, UInt32 locked)
        {
            if (password.Length != 8)
                return DL_STATUS.UFR_PARAMETERS_ERROR;

            StringBuilder ptr_password = new StringBuilder(password);
            return Linkage_EE_Lock(ptr_password, locked);
        }
    }
}
