using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diag_Doip_Uds.Lib.Doip_client.Common
{
    public class Common_doip_types
    {
        /* DoIP Port Number - Unsecured */
        public const UInt16 kDoipPort = 13400;
        /* Udp Channel Length */
        public const UInt32 kUdpChannelLength = 41U;
        /* Tcp Channel Length */
        public const UInt32 kTcpChannelLength = 4096U;
        /* DoIP Header */
        public const byte kDoipheadrSize = 0x8;
        public const byte kDoip_ProtocolVersion_2012 = 0x2;  // ISO 13400-2012
        public const byte kDoip_ProtocolVersion_2019 = 0x3;  // ISO 13400-2019
        public const byte kDoip_ProtocolVersion = kDoip_ProtocolVersion_2012;
        public const byte kDoip_ProtocolVersion_Def = 0xFF;
        public const UInt32 kDoip_Protocol_MaxPayload = 0xFFFFFFFF;  // 4294967295 bytes
        /* Payload Types */
        public const UInt16 kDoip_GenericHeadr_NackType = 0x0000;
        public const UInt16 kDoip_VehicleIdentification_ReqType = 0x0001;
        public const UInt16 kDoip_VehicleIdentificationEID_ReqType = 0x0002;
        public const UInt16 kDoip_VehicleIdentificationVIN_ReqType = 0x0003;
        public const UInt16 kDoip_VehicleAnnouncement_ResType = 0x0004;
        public const UInt16 kDoip_RoutingActivation_ReqType = 0x0005;
        public const UInt16 kDoip_RoutingActivation_ResType = 0x0006;
        public const UInt16 kDoip_AliveCheck_ReqType = 0x0007;
        public const UInt16 kDoip_AliveCheck_ResType = 0x0008;
        //constexpr UInt16 kDoipENTITY_STATUS_REQ_TYPE                             0x4001
        //constexpr UInt16 kDoipENTITY_STATUS_RES_TYPE                             0x4002
        //constexpr UInt16 kDoipDIAG_POWER_MODEINFO_REQ_TYPE                       0x4003
        //constexpr UInt16 kDoipDIAG_POWER_MODEINFO_RES_TYPE                       0x4004
        public const UInt16 kDoip_DiagMessage_Type = 0x8001;
        public const UInt16 kDoip_DiagMessagePosAck_Type = 0x8002;
        public const UInt16 kDoip_DiagMessageNegAck_Type = 0x8003;
        public const UInt16 kDoip_InvalidPayload_Type = 0xFFFF;
        /* Payload length excluding header */
        public const byte kDoip_VehicleIdentification_ReqLen = 0;
        public const byte kDoip_VehicleIdentificationEID_ReqLen = 6;
        public const byte kDoip_VehicleIdentificationVIN_ReqLen = 17;
        public const byte kDoip_VehicleAnnouncement_ResMaxLen = 33;
        public const byte kDoip_GenericHeader_NackLen = 1;
        public const byte kDoip_RoutingActivation_ReqMinLen = 7;   //without OEM specific use byte
        public const byte kDoip_RoutingActivation_ResMinLen = 9;   //without OEM specific use byte
        public const byte kDoip_RoutingActivation_ReqMaxLen = 11;  //with OEM specific use byte
        public const byte kDoip_RoutingActivation_ResMaxLen = 13;  //with OEM specific use byte
        //constexpr byte kDoipALIVE_CHECK_RES_LEN							1
        public const byte kDoip_DiagMessage_ReqResMinLen = 4;  // considering SA and TA
        public const byte kDoip_DiagMessageAck_ResMinLen = 5;  // considering SA, TA, Ack code
        /* Generic DoIP Header NACK codes */
        public const byte kDoip_GenericHeader_IncorrectPattern = 0x00;
        public const byte kDoip_GenericHeader_UnknownPayload = 0x01;
        public const byte kDoip_GenericHeader_MessageTooLarge = 0x02;
        public const byte kDoip_GenericHeader_OutOfMemory = 0x03;
        public const byte kDoip_GenericHeader_InvalidPayloadLen = 0x04;
        /* Routing Activation request activation types */
        public const byte kDoip_RoutingActivation_ReqActType_Default = 0x00;
        public const byte kDoip_RoutingActivation_ReqActType_WWHOBD = 0x01;
        public const byte kDoip_RoutingActivation_ReqActType_CentralSec = 0xE0;
        /* Routing Activation response code values */
        public const byte kDoip_RoutingActivation_ResCode_UnknownSA = 0x00;
        public const byte kDoip_RoutingActivation_ResCode_AllSocktActive = 0x01;
        public const byte kDoip_RoutingActivation_ResCode_DifferentSA = 0x02;
        public const byte kDoip_RoutingActivation_ResCode_ActiveSA = 0x03;
        public const byte kDoip_RoutingActivation_ResCode_AuthentnMissng = 0x04;
        public const byte kDoip_RoutingActivation_ResCode_ConfirmtnRejectd = 0x05;
        public const byte kDoip_RoutingActivation_ResCode_UnsupportdActType = 0x06;
        public const byte kDoip_RoutingActivation_ResCode_TLSRequired = 0x07;
        public const byte kDoip_RoutingActivation_ResCode_RoutingSuccessful = 0x10;
        public const byte kDoip_RoutingActivation_ResCode_ConfirmtnRequired = 0x11;
        /* Diagnostic Message positive acknowledgement code */
        public const byte kDoip_DiagnosticMessage_PosAckCode_Confirm = 0x00;
        /* Diagnostic Message negative acknowledgement code */
        public const byte kDoip_DiagnosticMessage_NegAckCode_InvalidSA = 0x02;
        public const byte kDoip_DiagnosticMessage_NegAckCode_UnknownTA = 0x03;
        public const byte kDoip_DiagnosticMessage_NegAckCode_MessageTooLarge = 0x04;
        public const byte kDoip_DiagnosticMessage_NegAckCode_OutOfMemory = 0x05;
        public const byte kDoip_DiagnosticMessage_NegAckCode_TargetUnreachable = 0x06;
        public const byte kDoip_DiagnosticMessage_NegAckCode_UnknownNetwork = 0x07;
        public const byte kDoip_DiagnosticMessage_NegAckCode_TPError = 0x08;

        /* Further action code values */
        //constexpr byte kDoipNO_FURTHER_ACTION                              0x00
        //constexpr byte kDoipFURTHER_ACTION_CENTRAL_SEC                     0x10
        //constexpr byte kDoipFURTHER_VM_SPECIFIC_MIN                        0x11
        //constexpr byte kDoipFURTHER_VM_SPECIFIC_MAX                        0xFF
        /* VIN/GID Sync status Code values */
        //constexpr byte kDoipVIN_GID_SYNC                                   0x00
        //constexpr byte kDoipVIN_GID_NOT_SYNC                               0x10
        /* Vehicle identification parameter (invalidity pattern) */
        public const byte kDoip_VIN_Invalid_FF = 0xFF;
        public const byte kDoip_VIN_Invalid_00 = 0x00;
        public const UInt16 kDoip_LogAddress_Invalid = 0xFFFF;
        public const byte kDoip_EID_Invalid_FF = 0xFF;
        public const byte kDoip_EID_Invalid_00 = 0x00;
        public const byte kDoip_GID_Invalid_FF = 0xFF;
        public const byte kDoip_GID_Invalid_00 = 0x00;
        /* DoIP timing and communication parameter (in millisecond) */
        /* Description: This is used to configure the
                timeout value for a DoIP Routing Activation request */
        public const UInt32 kDoIPRoutingActivationTimeout = 1000U;  // 1 sec
        /* Description: This timeout specifies the maximum time that
                the test equipment waits for a confirmation ACK or NACK
                from the DoIP entity after the last byte of a DoIP Diagnostic
                request message has been sent
        */
        public const UInt32 kDoIPDiagnosticAckTimeout = 2000U;  // 2 sec
        /* Description: This timeout specifies the maximum time that the
                client waits for response to a previously sent UDP message.
                This includes max time to wait and collect multiple responses
                to previous broadcast(UDP only)
        * */
        public const UInt32 kDoIPCtrl = 2000U;  // 2 sec
    }
}
