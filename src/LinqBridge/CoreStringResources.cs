using System.Globalization;
using System.Resources;
using System.Threading;

namespace System
{
    internal sealed class CoreStringResources
    {
        internal const string ArgumentOutOfRange_NeedNonNegNum = "ArgumentOutOfRange_NeedNonNegNum";

        internal const string Argument_AdjustmentRulesNoNulls = "Argument_AdjustmentRulesNoNulls";

        internal const string Argument_AdjustmentRulesOutOfOrder = "Argument_AdjustmentRulesOutOfOrder";

        internal const string Argument_AdjustmentRulesAmbiguousOverlap = "Argument_AdjustmentRulesAmbiguousOverlap";

        internal const string Argument_AdjustmentRulesrDaylightSavingTimeOverlap = "Argument_AdjustmentRulesrDaylightSavingTimeOverlap";

        internal const string Argument_AdjustmentRulesrDaylightSavingTimeOverlapNonRuleRange = "Argument_AdjustmentRulesrDaylightSavingTimeOverlapNonRuleRange";

        internal const string Argument_AdjustmentRulesInvalidOverlap = "Argument_AdjustmentRulesInvalidOverlap";

        internal const string Argument_ConvertMismatch = "Argument_ConvertMismatch";

        internal const string Argument_DateTimeHasTimeOfDay = "Argument_DateTimeHasTimeOfDay";

        internal const string Argument_DateTimeIsInvalid = "Argument_DateTimeIsInvalid";

        internal const string Argument_DateTimeIsNotAmbiguous = "Argument_DateTimeIsNotAmbiguous";

        internal const string Argument_DateTimeOffsetIsNotAmbiguous = "Argument_DateTimeOffsetIsNotAmbiguous";

        internal const string Argument_DateTimeKindMustBeUnspecified = "Argument_DateTimeKindMustBeUnspecified";

        internal const string Argument_DateTimeHasTicks = "Argument_DateTimeHasTicks";

        internal const string Argument_InvalidId = "Argument_InvalidId";

        internal const string Argument_InvalidSerializedString = "Argument_InvalidSerializedString";

        internal const string Argument_InvalidREG_TZI_FORMAT = "Argument_InvalidREG_TZI_FORMAT";

        internal const string Argument_OutOfOrderDateTimes = "Argument_OutOfOrderDateTimes";

        internal const string Argument_TimeSpanHasSeconds = "Argument_TimeSpanHasSeconds";

        internal const string Argument_TransitionTimesAreIdentical = "Argument_TransitionTimesAreIdentical";

        internal const string ArgumentOutOfRange_Day = "ArgumentOutOfRange_Day";

        internal const string ArgumentOutOfRange_DayOfWeek = "ArgumentOutOfRange_DayOfWeek";

        internal const string ArgumentOutOfRange_Month = "ArgumentOutOfRange_Month";

        internal const string ArgumentOutOfRange_UtcOffset = "ArgumentOutOfRange_UtcOffset";

        internal const string ArgumentOutOfRange_UtcOffsetAndDaylightDelta = "ArgumentOutOfRange_UtcOffsetAndDaylightDelta";

        internal const string ArgumentOutOfRange_Week = "ArgumentOutOfRange_Week";

        internal const string InvalidTimeZone_InvalidRegistryData = "InvalidTimeZone_InvalidRegistryData";

        internal const string InvalidTimeZone_InvalidWin32APIData = "InvalidTimeZone_InvalidWin32APIData";

        internal const string Security_CannotReadRegistryData = "Security_CannotReadRegistryData";

        internal const string Serialization_CorruptField = "Serialization_CorruptField";

        internal const string Serialization_InvalidData = "Serialization_InvalidData";

        internal const string Serialization_InvalidEscapeSequence = "Serialization_InvalidEscapeSequence";

        internal const string TimeZoneNotFound_MissingRegistryData = "TimeZoneNotFound_MissingRegistryData";

        internal const string Argument_WrongAsyncResult = "Argument_WrongAsyncResult";

        internal const string Argument_InvalidOffLen = "Argument_InvalidOffLen";

        internal const string Argument_NeedNonemptyPipeName = "Argument_NeedNonemptyPipeName";

        internal const string Argument_EmptyServerName = "Argument_EmptyServerName";

        internal const string Argument_NonContainerInvalidAnyFlag = "Argument_NonContainerInvalidAnyFlag";

        internal const string ArgumentNull_Buffer = "ArgumentNull_Buffer";

        internal const string ArgumentNull_ServerName = "ArgumentNull_ServerName";

        internal const string ArgumentOutOfRange_AdditionalAccessLimited = "ArgumentOutOfRange_AdditionalAccessLimited";

        internal const string ArgumentOutOfRange_AnonymousReserved = "ArgumentOutOfRange_AnonymousReserved";

        internal const string ArgumentOutOfRange_TransmissionModeByteOrMsg = "ArgumentOutOfRange_TransmissionModeByteOrMsg";

        internal const string ArgumentOutOfRange_DirectionModeInOrOut = "ArgumentOutOfRange_DirectionModeInOrOut";

        internal const string ArgumentOutOfRange_DirectionModeInOutOrInOut = "ArgumentOutOfRange_DirectionModeInOutOrInOut";

        internal const string ArgumentOutOfRange_ImpersonationInvalid = "ArgumentOutOfRange_ImpersonationInvalid";

        internal const string ArgumentOutOfRange_ImpersonationOptionsInvalid = "ArgumentOutOfRange_ImpersonationOptionsInvalid";

        internal const string ArgumentOutOfRange_OptionsInvalid = "ArgumentOutOfRange_OptionsInvalid";

        internal const string ArgumentOutOfRange_HandleInheritabilityNoneOrInheritable = "ArgumentOutOfRange_HandleInheritabilityNoneOrInheritable";

        internal const string ArgumentOutOfRange_InvalidPipeAccessRights = "ArgumentOutOfRange_InvalidPipeAccessRights";

        internal const string ArgumentOutOfRange_InvalidTimeout = "ArgumentOutOfRange_InvalidTimeout";

        internal const string ArgumentOutOfRange_MaxNumServerInstances = "ArgumentOutOfRange_MaxNumServerInstances";

        internal const string ArgumentOutOfRange_NeedValidPipeAccessRights = "ArgumentOutOfRange_NeedValidPipeAccessRights";

        internal const string IndexOutOfRange_IORaceCondition = "IndexOutOfRange_IORaceCondition";

        internal const string InvalidOperation_EndReadCalledMultiple = "InvalidOperation_EndReadCalledMultiple";

        internal const string InvalidOperation_EndWriteCalledMultiple = "InvalidOperation_EndWriteCalledMultiple";

        internal const string InvalidOperation_EndWaitForConnectionCalledMultiple = "InvalidOperation_EndWaitForConnectionCalledMultiple";

        internal const string InvalidOperation_PipeNotYetConnected = "InvalidOperation_PipeNotYetConnected";

        internal const string InvalidOperation_PipeDisconnected = "InvalidOperation_PipeDisconnected";

        internal const string InvalidOperation_PipeHandleNotSet = "InvalidOperation_PipeHandleNotSet";

        internal const string InvalidOperation_PipeNotAsync = "InvalidOperation_PipeNotAsync";

        internal const string InvalidOperation_PipeReadModeNotMessage = "InvalidOperation_PipeReadModeNotMessage";

        internal const string InvalidOperation_PipeMessageTypeNotSupported = "InvalidOperation_PipeMessageTypeNotSupported";

        internal const string InvalidOperation_PipeAlreadyConnected = "InvalidOperation_PipeAlreadyConnected";

        internal const string InvalidOperation_PipeAlreadyDisconnected = "InvalidOperation_PipeAlreadyDisconnected";

        internal const string InvalidOperation_PipeClosed = "InvalidOperation_PipeClosed";

        internal const string IO_FileTooLongOrHandleNotSync = "IO_FileTooLongOrHandleNotSync";

        internal const string IO_EOF_ReadBeyondEOF = "IO_EOF_ReadBeyondEOF";

        internal const string IO_FileNotFound = "IO_FileNotFound";

        internal const string IO_FileNotFound_FileName = "IO_FileNotFound_FileName";

        internal const string IO_IO_AlreadyExists_Name = "IO_IO_AlreadyExists_Name";

        internal const string IO_IO_BindHandleFailed = "IO_IO_BindHandleFailed";

        internal const string IO_IO_FileExists_Name = "IO_IO_FileExists_Name";

        internal const string IO_IO_NoPermissionToDirectoryName = "IO_IO_NoPermissionToDirectoryName";

        internal const string IO_IO_SharingViolation_File = "IO_IO_SharingViolation_File";

        internal const string IO_IO_SharingViolation_NoFileName = "IO_IO_SharingViolation_NoFileName";

        internal const string IO_IO_PipeBroken = "IO_IO_PipeBroken";

        internal const string IO_IO_InvalidPipeHandle = "IO_IO_InvalidPipeHandle";

        internal const string IO_DriveNotFound_Drive = "IO_DriveNotFound_Drive";

        internal const string IO_PathNotFound_Path = "IO_PathNotFound_Path";

        internal const string IO_PathNotFound_NoPathName = "IO_PathNotFound_NoPathName";

        internal const string IO_PathTooLong = "IO_PathTooLong";

        internal const string NotSupported_IONonFileDevices = "NotSupported_IONonFileDevices";

        internal const string NotSupported_MemStreamNotExpandable = "NotSupported_MemStreamNotExpandable";

        internal const string NotSupported_UnreadableStream = "NotSupported_UnreadableStream";

        internal const string NotSupported_UnseekableStream = "NotSupported_UnseekableStream";

        internal const string NotSupported_UnwritableStream = "NotSupported_UnwritableStream";

        internal const string NotSupported_AnonymousPipeUnidirectional = "NotSupported_AnonymousPipeUnidirectional";

        internal const string NotSupported_AnonymousPipeMessagesNotSupported = "NotSupported_AnonymousPipeMessagesNotSupported";

        internal const string ObjectDisposed_FileClosed = "ObjectDisposed_FileClosed";

        internal const string ObjectDisposed_PipeClosed = "ObjectDisposed_PipeClosed";

        internal const string ObjectDisposed_ReaderClosed = "ObjectDisposed_ReaderClosed";

        internal const string ObjectDisposed_StreamClosed = "ObjectDisposed_StreamClosed";

        internal const string ObjectDisposed_WriterClosed = "ObjectDisposed_WriterClosed";

        internal const string PlatformNotSupported_NamedPipeServers = "PlatformNotSupported_NamedPipeServers";

        internal const string UnauthorizedAccess_IODenied_Path = "UnauthorizedAccess_IODenied_Path";

        internal const string UnauthorizedAccess_IODenied_NoPathName = "UnauthorizedAccess_IODenied_NoPathName";

        internal const string TraceAsTraceSource = "TraceAsTraceSource";

        internal const string ArgumentOutOfRange_NeedValidLogRetention = "ArgumentOutOfRange_NeedValidLogRetention";

        internal const string ArgumentOutOfRange_NeedMaxFileSizeGEBufferSize = "ArgumentOutOfRange_NeedMaxFileSizeGEBufferSize";

        internal const string ArgumentOutOfRange_NeedValidMaxNumFiles = "ArgumentOutOfRange_NeedValidMaxNumFiles";

        internal const string ArgumentOutOfRange_NeedValidId = "ArgumentOutOfRange_NeedValidId";

        internal const string ArgumentOutOfRange_MaxArgExceeded = "ArgumentOutOfRange_MaxArgExceeded";

        internal const string ArgumentOutOfRange_MaxStringsExceeded = "ArgumentOutOfRange_MaxStringsExceeded";

        internal const string NotSupported_DownLevelVista = "NotSupported_DownLevelVista";

        internal const string Argument_NeedNonemptyDelimiter = "Argument_NeedNonemptyDelimiter";

        internal const string NotSupported_SetTextWriter = "NotSupported_SetTextWriter";

        internal const string Perflib_PlatformNotSupported = "Perflib_PlatformNotSupported";

        internal const string Perflib_Argument_CounterSetAlreadyRegister = "Perflib_Argument_CounterSetAlreadyRegister";

        internal const string Perflib_Argument_InvalidCounterType = "Perflib_Argument_InvalidCounterType";

        internal const string Perflib_Argument_InvalidCounterSetInstanceType = "Perflib_Argument_InvalidCounterSetInstanceType";

        internal const string Perflib_Argument_InstanceAlreadyExists = "Perflib_Argument_InstanceAlreadyExists";

        internal const string Perflib_Argument_CounterAlreadyExists = "Perflib_Argument_CounterAlreadyExists";

        internal const string Perflib_Argument_CounterNameAlreadyExists = "Perflib_Argument_CounterNameAlreadyExists";

        internal const string Perflib_Argument_ProviderNotFound = "Perflib_Argument_ProviderNotFound";

        internal const string Perflib_Argument_InvalidInstance = "Perflib_Argument_InvalidInstance";

        internal const string Perflib_Argument_EmptyInstanceName = "Perflib_Argument_EmptyInstanceName";

        internal const string Perflib_Argument_EmptyCounterName = "Perflib_Argument_EmptyCounterName";

        internal const string Perflib_InsufficientMemory_InstanceCounterBlock = "Perflib_InsufficientMemory_InstanceCounterBlock";

        internal const string Perflib_InsufficientMemory_CounterSetTemplate = "Perflib_InsufficientMemory_CounterSetTemplate";

        internal const string Perflib_InvalidOperation_CounterRefValue = "Perflib_InvalidOperation_CounterRefValue";

        internal const string Perflib_InvalidOperation_CounterSetNotInstalled = "Perflib_InvalidOperation_CounterSetNotInstalled";

        internal const string Perflib_InvalidOperation_InstanceNotFound = "Perflib_InvalidOperation_InstanceNotFound";

        internal const string Perflib_InvalidOperation_AddCounterAfterInstance = "Perflib_InvalidOperation_AddCounterAfterInstance";

        internal const string Perflib_InvalidOperation_NoActiveProvider = "Perflib_InvalidOperation_NoActiveProvider";

        internal const string Perflib_InvalidOperation_CounterSetContainsNoCounter = "Perflib_InvalidOperation_CounterSetContainsNoCounter";

        internal const string Arg_ArrayPlusOffTooSmall = "Arg_ArrayPlusOffTooSmall";

        internal const string Arg_HSCapacityOverflow = "Arg_HSCapacityOverflow";

        internal const string InvalidOperation_EnumFailedVersion = "InvalidOperation_EnumFailedVersion";

        internal const string InvalidOperation_EnumOpCantHappen = "InvalidOperation_EnumOpCantHappen";

        internal const string Serialization_MissingKeys = "Serialization_MissingKeys";

        internal const string LockRecursionException_RecursiveReadNotAllowed = "LockRecursionException_RecursiveReadNotAllowed";

        internal const string LockRecursionException_RecursiveWriteNotAllowed = "LockRecursionException_RecursiveWriteNotAllowed";

        internal const string LockRecursionException_RecursiveUpgradeNotAllowed = "LockRecursionException_RecursiveUpgradeNotAllowed";

        internal const string LockRecursionException_ReadAfterWriteNotAllowed = "LockRecursionException_ReadAfterWriteNotAllowed";

        internal const string LockRecursionException_WriteAfterReadNotAllowed = "LockRecursionException_WriteAfterReadNotAllowed";

        internal const string LockRecursionException_UpgradeAfterReadNotAllowed = "LockRecursionException_UpgradeAfterReadNotAllowed";

        internal const string LockRecursionException_UpgradeAfterWriteNotAllowed = "LockRecursionException_UpgradeAfterWriteNotAllowed";

        internal const string SynchronizationLockException_MisMatchedRead = "SynchronizationLockException_MisMatchedRead";

        internal const string SynchronizationLockException_MisMatchedWrite = "SynchronizationLockException_MisMatchedWrite";

        internal const string SynchronizationLockException_MisMatchedUpgrade = "SynchronizationLockException_MisMatchedUpgrade";

        internal const string SynchronizationLockException_IncorrectDispose = "SynchronizationLockException_IncorrectDispose";

        internal const string Cryptography_ArgECDHKeySizeMismatch = "Cryptography_ArgECDHKeySizeMismatch";

        internal const string Cryptography_ArgECDHRequiresECDHKey = "Cryptography_ArgECDHRequiresECDHKey";

        internal const string Cryptography_ArgECDsaRequiresECDsaKey = "Cryptography_ArgECDsaRequiresECDsaKey";

        internal const string Cryptography_ArgExpectedECDiffieHellmanCngPublicKey = "Cryptography_ArgExpectedECDiffieHellmanCngPublicKey";

        internal const string Cryptography_ArgMustBeCngAlgorithm = "Cryptography_ArgMustBeCngAlgorithm";

        internal const string Cryptography_ArgMustBeCngAlgorithmGroup = "Cryptography_ArgMustBeCngAlgorithmGroup";

        internal const string Cryptography_ArgMustBeCngKeyBlobFormat = "Cryptography_ArgMustBeCngKeyBlobFormat";

        internal const string Cryptography_ArgMustBeCngProvider = "Cryptography_ArgMustBeCngProvider";

        internal const string Cryptography_DecryptWithNoKey = "Cryptography_DecryptWithNoKey";

        internal const string Cryptography_ECXmlSerializationFormatRequired = "Cryptography_ECXmlSerializationFormatRequired";

        internal const string Cryptography_InvalidAlgorithmGroup = "Cryptography_InvalidAlgorithmGroup";

        internal const string Cryptography_InvalidAlgorithmName = "Cryptography_InvalidAlgorithmName";

        internal const string Cryptography_InvalidCipherMode = "Cryptography_InvalidCipherMode";

        internal const string Cryptography_InvalidIVSize = "Cryptography_InvalidIVSize";

        internal const string Cryptography_InvalidKeyBlobFormat = "Cryptography_InvalidKeyBlobFormat";

        internal const string Cryptography_InvalidKeySize = "Cryptography_InvalidKeySize";

        internal const string Cryptography_InvalidPadding = "Cryptography_InvalidPadding";

        internal const string Cryptography_InvalidProviderName = "Cryptography_InvalidProviderName";

        internal const string Cryptography_MissingDomainParameters = "Cryptography_MissingDomainParameters";

        internal const string Cryptography_MissingPublicKey = "Cryptography_MissingPublicKey";

        internal const string Cryptography_MissingIV = "Cryptography_MissingIV";

        internal const string Cryptography_MustTransformWholeBlock = "Cryptography_MustTransformWholeBlock";

        internal const string Cryptography_NonCompliantFIPSAlgorithm = "Cryptography_NonCompliantFIPSAlgorithm";

        internal const string Cryptography_OpenInvalidHandle = "Cryptography_OpenInvalidHandle";

        internal const string Cryptography_OpenEphemeralKeyHandleWithoutEphemeralFlag = "Cryptography_OpenEphemeralKeyHandleWithoutEphemeralFlag";

        internal const string Cryptography_PartialBlock = "Cryptography_PartialBlock";

        internal const string Cryptography_PlatformNotSupported = "Cryptography_PlatformNotSupported";

        internal const string Cryptography_TlsRequiresLabelAndSeed = "Cryptography_TlsRequiresLabelAndSeed";

        internal const string Cryptography_TransformBeyondEndOfBuffer = "Cryptography_TransformBeyondEndOfBuffer";

        internal const string Cryptography_UnknownEllipticCurve = "Cryptography_UnknownEllipticCurve";

        internal const string Cryptography_UnknownEllipticCurveAlgorithm = "Cryptography_UnknownEllipticCurveAlgorithm";

        internal const string Cryptography_UnknownPaddingMode = "Cryptography_UnknownPaddingMode";

        internal const string Cryptography_UnexpectedXmlNamespace = "Cryptography_UnexpectedXmlNamespace";

        private static CoreStringResources loader;

        private ResourceManager resources;

        private static CultureInfo Culture
        {
            get
            {
                return null;
            }
        }

        public static ResourceManager Resources
        {
            get
            {
                return CoreStringResources.GetLoader().resources;
            }
        }

        internal CoreStringResources()
        {
            this.resources = new ResourceManager("System.Core", this.GetType().Assembly);
        }

        private static CoreStringResources GetLoader()
        {
            if (CoreStringResources.loader == null)
            {
                CoreStringResources sR = new CoreStringResources();
                Net20Interlocked.CompareExchange<CoreStringResources>(ref CoreStringResources.loader, sR, null);
            }
            return CoreStringResources.loader;
        }

        public static object GetObject(string name)
        {
            CoreStringResources loader = CoreStringResources.GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetObject(name, CoreStringResources.Culture);
        }

        public static string GetString(string name, params object[] args)
        {
            CoreStringResources loader = CoreStringResources.GetLoader();
            if (loader == null)
            {
                return null;
            }
            string str = loader.resources.GetString(name, CoreStringResources.Culture);
            if (args == null || (int)args.Length <= 0)
            {
                return str;
            }
            for (int i = 0; i < (int)args.Length; i++)
            {
                string str1 = args[i] as string;
                if (str1 != null && str1.Length > 1024)
                {
                    args[i] = string.Concat(str1.Substring(0, 1021), "...");
                }
            }
            return string.Format(CultureInfo.CurrentCulture, str, args);
        }

        public static string GetString(string name)
        {
            CoreStringResources loader = CoreStringResources.GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetString(name, CoreStringResources.Culture);
        }
    }
}