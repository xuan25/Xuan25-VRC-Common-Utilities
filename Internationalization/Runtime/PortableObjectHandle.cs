
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.Internationalization
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class PortableObjectHandle : LocaleHandle
    {
        [SerializeField]
        [Tooltip("The .po.txt file to load translations from. The file should be in UTF-8 encoding and follow the standard .po format.")]
        public TextAsset portableObjectFile;

        [SerializeField]
        [Tooltip("The header key to use for identifying the language of this locale file.")]
        public string localeHeaderKey = "Language";
        
        [SerializeField]
        [Tooltip("Override the language ID for this locale file in RFC 5646 format.")]
        public string localeOverride = "";

        VRC.SDK3.Data.DataDictionary translations = new VRC.SDK3.Data.DataDictionary();
        VRC.SDK3.Data.DataDictionary headers = new VRC.SDK3.Data.DataDictionary();

        bool translationsLoaded = false;
        bool headersLoaded = false;

        public override bool GetText(string id, out string msg)
        {
            if (!EnsureTranslations())
            {
                Debug.LogError($"[{nameof(PortableObjectHandle)}] Failed to load translations. GameObject: {gameObject.name}");
                msg = id;
                return false;
            }

            if (!translations.TryGetValue(id, VRC.SDK3.Data.TokenType.String, out VRC.SDK3.Data.DataToken value))
            {
                Debug.LogWarning($"[{nameof(PortableObjectHandle)}] No translation found for key: {id}. GameObject: {gameObject.name}");
                msg = id;
                return false;
            }

            msg = value.String;
            return true;
        }

        public bool GetHeader(string id, out string header)
        {
            if (!EnsureHeaders())
            {
                Debug.LogError($"[{nameof(PortableObjectHandle)}] Failed to load headers. GameObject: {gameObject.name}");
                header = id;
                return false;
            }

            if (!headers.TryGetValue(id, VRC.SDK3.Data.TokenType.String, out VRC.SDK3.Data.DataToken value))
            {
                Debug.LogWarning($"[{nameof(PortableObjectHandle)}] No header found for key: {id}. GameObject: {gameObject.name}");
                header = id;
                return false;
            }
            
            header = value.String;
            return true;
        }

        public override bool GetLanguageID(out string language)
        {
            if (localeOverride != "")
            {
                language = localeOverride;
                return true;
            }

            if (GetHeader(localeHeaderKey, out language))
            {
                return true;
            }

            Debug.LogError($"[{nameof(PortableObjectHandle)}] Failed to get language info. GameObject: {gameObject.name}");
            return false;
        }
        
        private bool EnsureTranslations()
        {
            if (translationsLoaded) return true;

            if (portableObjectFile == null)
            {
                Debug.LogError($"[{nameof(PortableObjectHandle)}] No portable object file assigned. GameObject: {gameObject.name}");
                return false;
            }

            int code = PortableObjectParser.ParseTranslations(portableObjectFile.text, translations);
            if (code != 0)
            {
                Debug.LogError($"[{nameof(PortableObjectHandle)}] Failed to parse portable object file @ Line: {code}. GameObject: {gameObject.name}");
                return false;
            }

            translationsLoaded = true;
            return true;
        }

        private bool EnsureHeaders()
        {
            if (headersLoaded) return true;

            if(!EnsureTranslations())
            {
                Debug.LogError($"[{nameof(PortableObjectHandle)}] Failed to parse translations. GameObject: {gameObject.name}");
                return false;
            }

            if (!translations.TryGetValue("", VRC.SDK3.Data.TokenType.String, out VRC.SDK3.Data.DataToken headerContent))
            {   
                Debug.LogError($"[{nameof(PortableObjectHandle)}] No header entry found in translations. GameObject: {gameObject.name}");
                return false;
            }

            int code = PortableObjectParser.ParseHeaders(headerContent.String, headers);
            if (code != 0)
            {
                Debug.LogError($"[{nameof(PortableObjectHandle)}] Failed to parse headers. @ Line: {code}. GameObject: {gameObject.name}");
                return false;
            }

            headersLoaded = true;
            return true;
        }

        public bool Bake()
        {
            if (portableObjectFile == null)
                return false;

            EnsureTranslations();
            EnsureHeaders();
            return true;
        }
    }
}
