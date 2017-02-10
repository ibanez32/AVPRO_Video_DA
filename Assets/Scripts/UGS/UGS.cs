using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
//using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Security.Policy;
using LitJsonSrc;
using UnityEngine;

namespace UGS
{
    public enum SortingType
    {
        Name,
        Creation,
        Modification,
        Popularity,
        Rating
    }

    public enum FilteringType
    {
        All,
        My,
        Best,
        Section,
        Category,
        Author,
        Owner
    }

    public enum UGSObjects
    {
        Games,
        Lessons,
        Books,
    }

    public struct FileMetadata
    {
        public string Bucket;
        public string Key;
        public string Version;
        public int Size;

        public FileDownloadProcedure Download()
        {
            return new FileDownloadProcedure(this);
        }
    }

    public abstract class BaseObject
    {
        public FileMetadata[] Files { get; private set; }

        public string Id { get; private set; }
        public DateTime Created { get; private set; }
        public DateTime CheckedOut { get; private set; }
        public DateTime Commit { get; private set; }

        public virtual void Init(JsonData data)
        {
            Id = data["id"].AsString();
            Created = DateTime.Parse(data["created"].AsString());
            CheckedOut = DateTime.Parse(data["checkedout"].AsString());
            Commit = DateTime.Parse(data["commit"].AsString());

            var files = new List<FileMetadata>();
            var list = data["files"];
            for (var index = 0; index < list.Count; index++)
            {
                var fileInfo = list[index];
                files.Add(new FileMetadata
                {
                    Bucket = fileInfo["bucket"].AsString(),
                    Key = fileInfo["key"].AsString(),
                    Version = fileInfo["VersionId"].AsString(),
                    Size = fileInfo["size"].AsInt(),
                });
            }
            Files = files.ToArray();
        }
    }

    public class Lesson : BaseObject
    {
        public string Name { get; private set; }
        public string AuthorName { get; private set; }
        public string Section { get; private set; }
        public string Category { get; private set; }
        public string Language { get; private set; }

        public int GetSize()
        {
            foreach (var f in Files)
            {
                if (f.Key.Contains(".um24"))
                {
                    return f.Size;
                }
            }
            return -1;
        }

        public override void Init(JsonData data)
        {
            base.Init(data);
            var meta = data["meta"];
            Name = meta["LessonName"].AsString();
            AuthorName = meta["AuthorName"].AsString();
            Language = meta["Lang"].AsString();
            Section = meta["Section"].AsString();
            Category = meta["Category"].AsString();
        }
    }

   

    public class HttpError : Exception
    {
        public HttpError(string error)
            : base(error)
        {
        }
    }

    public class LoginAlreadyOccupiedError : Exception
    {
        public LoginAlreadyOccupiedError(string login)
            : base(login)
        {
        }
    }

    public class InvalidLoginOrPasswordError : Exception
    {
        public InvalidLoginOrPasswordError()
        {
        }
    }

    public class AssertionError : Exception
    {
    }

   // internal static class Extensions
  // {
  //     public static string AsString(this JsonData data)
  //     {
  //         return (string) data;
  //     }
  //
  //     public static long AsLong(this JsonData data)
  //     {
  //         return (long) data;
  //     }
  //
  //     public static int AsInt(this JsonData data)
  //     {
  //         return (int) data;
  //     }
  //
  //     public static double AsDouble(this JsonData data)
  //     {
  //         return (double) data;
  //     }
  //
  //     public static bool AsBool(this JsonData data)
  //     {
  //         return (bool) data;
  //     }
  //
  // }

    public abstract class Procedure<T>
    {
        public event Action<T> Done;

        public void OnDone(T result)
        {
            if (Done != null) Done(result);
        }

        public event Action<Exception> Fail;

        public void OnFail(Exception error)
        {
            if (Fail != null) Fail(error);
        }

        protected void Assert(bool condition)
        {
            if (condition) return;
            throw new AssertionError();
        }

        public abstract IEnumerator Process();
        protected abstract T GetResult();
    }

    public abstract class RequestProcedure<T> : Procedure<T>
    {
        public WWW Request { get; protected set; }

        public T Result { get; private set; }
        public bool IsDone { get; private set; }
        public bool HasError { get; private set; }

        public void DebugLog(string format, params object[] @params)
        {
            if (Debug.isDebugBuild)
            {
                var text = string.Format(format, @params);
                Debug.Log(text);
            }
        }

        public override IEnumerator Process()
        {
            DebugLog(" --> {0}", Request.url);
            //DebugLog(Request.);
            yield return Request;
            IsDone = true;
            try
            {
                if (!string.IsNullOrEmpty(Request.error))
                {
                    DebugLog(" <-- FAIL: {0}", Request.error);
                    throw new HttpError(Request.error);
                }

                Result = GetResult();
                DebugLog(" <-- SUCCESS: {0}", Request.text.Length > 128 ? Request.text.Substring(0, 128) : Request.text);
                OnDone(Result);
            }
            catch (Exception error)
            {
                DebugLog(" !!! FAIL: {0}", error);
                HasError = true;
                OnFail(error);
            }
        }
    }

    public abstract class UGSRequestProcedure<T> : RequestProcedure<T>
    {

        protected bool IsError(JsonData response)
        {
            Assert(response.IsArray);
            Assert(response.Count > 0);
            Assert(response[0].IsBoolean);

            if (response[0].AsBool())
            {
                Assert(response.Count == 3);
                Assert(response[1].IsArray);
                Assert(response[2].IsObject);
                return false;
            }

            Assert(response.Count == 4);
            Assert(response[1].IsString);
            Assert(response[2].IsArray);
            Assert(response[3].IsObject);
            return true;
        }

        protected string MakeURL(params string[] segments)
        {
            var s = Client.URL;
            //Debug.Log(s);
            return s;
        }

        protected string AddParams(string url, Dictionary<string, string> @params)
        {
            var sparams = string.Join("&", @params.Select(param => param.Key + "=" + param.Value).ToArray());
            return @params.Count == 0 ? url : url + "?" + sparams;
        }

        protected UgsClient Client;

        protected UGSRequestProcedure(UgsClient client)
        {
            Client = client;
        }

        protected string EncodePassword(string password)
        {
            var md5 = new MD5.MD5 {Value = password};
            return md5.FingerPrint;
        }
    }

    public class Auth_PIN : UGSRequestProcedure<string>
    {
        internal Auth_PIN(UgsClient client, string mac_adress)
            : base(client)
        {



            var url = MakeURL(mac_adress);
            //DebugLog(form.data.OrderByDescending(`));
           

            WWWForm form = new WWWForm();
            Dictionary<string, string> headers = form.headers;
            headers["device-mac"] = mac_adress;
       
            Request = new WWW(url, null, headers);
        }
        protected override string GetResult()
        {
            var response = Request.text;
          //  Debug.Log("Response:" + response);
            return response;
        }
    }
    public class LoginProcedure : UGSRequestProcedure<string>
    {
        private readonly string _login;

        internal LoginProcedure(UgsClient client, string login, string password)
            : base(client)
        {
            _login = login;
            var form = new WWWForm();
            form.AddField("login", login);
            form.AddField("password", EncodePassword(password));

            var url = MakeURL("auth", "login");
            //DebugLog(form.data.OrderByDescending(`));
            Request = new WWW(url, form);
        }

        protected override string GetResult()
        {
            Debug.Log(Request.text);
            var response = JsonMapper.ToObject(Request.text);

            
            if (IsError(response))
            {
                switch (response[1].AsString())
                {
                    case "InvalidLoginOrPassword":
                        throw new InvalidLoginOrPasswordError();
                    default:
                        throw new NotImplementedException();
                }
            }

            Client.Logged = true;
            Client.Session = response[1][0].AsString();
            Client.UserLogin = _login;

            return Client.Session;
        }
    }

   

    public class ChangeCredentialsProcedure : UGSRequestProcedure<bool>
    {
        private readonly string _login;

        public ChangeCredentialsProcedure(UgsClient client, string login, string password) : base(client)
        {
            var url = MakeURL(Client.Session, "auth", "change");
            var form = new WWWForm();
            _login = login;
            form.AddField("login", login);
            form.AddField("password", EncodePassword(password));
            Request = new WWW(url, form);
        }

        protected override bool GetResult()
        {
            var response = JsonMapper.ToObject(Request.text);

            if (IsError(response))
            {
                switch (response[1].AsString())
                {
                    case "LoginAlreadyOccupied":
                        throw new LoginAlreadyOccupiedError(_login);
                    default:
                        throw new NotImplementedException();
                }
            }

            return true;
        }
    }

    public class AchievementsProcedure : UGSRequestProcedure<string>
    {
        internal AchievementsProcedure(UgsClient client, string iCategory)
            : base(client)
        {
            var url = MakeURL(client.Session, "player", "achievments", iCategory);
            Request = new WWW(url);
            Debug.Log("URL:" + url);
        }

        protected override string GetResult()
        {
            var response = Request.text;
            Debug.Log("Response:" + response);
            return response;
        }
    }

    public class CharConfig
    {
        public string config;

        public CharConfig(string text)
        {
            config = text;
        }
    }

    public class CharacterViewProcedure : UGSRequestProcedure<CharConfig>
    {
        public CharacterViewProcedure(UgsClient client) : base(client)
        {
            var url = MakeURL(client.Session, "player");
            Request = new WWW(url);
            Debug.Log("URL:" + url);
        }

        protected override CharConfig GetResult()
        {
            var response = Request.text;
            Debug.Log("Response:" + response);
            return new CharConfig(response);
        }
    }

    public class CharacterSaveProcedure : UGSRequestProcedure<string>
    {
        public CharacterSaveProcedure(UgsClient client, string newConfig) : base(client)
        {
            var url = MakeURL(client.Session, "player", "character");
            Debug.Log("set_url:" + url);
            var form = new WWWForm();
            form.AddField("character", newConfig);
            Request = new WWW(url, form);
        }

        protected override string GetResult()
        {
            var response = Request.text;
            Debug.Log("Response:" + response);
            return response;
        }
    }

    public class LessonViewProcedure : UGSRequestProcedure<Lesson>
    {
        public LessonViewProcedure(UgsClient client, string lessonId, bool lf) : base(client)
        {
            var url = lf?
                MakeURL("lessonfile", lessonId) :
                MakeURL("lesson", lessonId);
            Request = new WWW(url);
        }

        protected override Lesson GetResult()
        {
            var response = JsonMapper.ToObject(Request.text);
            var lesson = new Lesson();
            lesson.Init(response[1][0]);
            return lesson;
        }
    }

    public class LessonCreateProcedure : UGSRequestProcedure<string>
    {
        internal LessonCreateProcedure(UgsClient client, bool lf)
            : base(client)
        {

            var url = lf?
                    MakeURL(Client.Session, "lessonfile", "create")
                    : MakeURL(Client.Session, "lesson", "create");
            Request = new WWW(url);
        }

        protected override string GetResult()
        {
            var response = JsonMapper.ToObject(Request.text);

            if (IsError(response))
            {
                throw new NotImplementedException();
            }

            return response[1][0].AsString();
        }
    }

    public class LessonHideProcedure : UGSRequestProcedure<bool>
    {
        public readonly string LessonId;

        internal LessonHideProcedure(UgsClient client, string lessonId, bool lf)
            : base(client)
        {
            LessonId = lessonId;
            var url = lf?
                MakeURL(Client.Session, "lessonfile", LessonId, "hide")
                : MakeURL(Client.Session, "lesson", LessonId, "hide");
            Request = new WWW(url);
        }

        protected override bool GetResult()
        {
            var response = JsonMapper.ToObject(Request.text);

            if (IsError(response))
            {
                throw new NotImplementedException();
            }

            return true;
        }
    }

    public class LessonUnhideProcedure : UGSRequestProcedure<bool>
    {
        public readonly string LessonId;

        internal LessonUnhideProcedure(UgsClient client, string lessonId, bool lf)
            : base(client)
        {
            LessonId = lessonId;
            var url = lf?
                MakeURL(Client.Session, "lessonfile", LessonId, "unhide")
                : MakeURL(Client.Session, "lesson", LessonId, "unhide");
            Request = new WWW(url);
        }

        protected override bool GetResult()
        {
            var response = JsonMapper.ToObject(Request.text);

            if (IsError(response))
            {
                throw new NotImplementedException();
            }

            return true;
        }
    }

    public class SetStateProcedure : UGSRequestProcedure<bool>
    {
        public readonly string GameID;

        internal SetStateProcedure(UgsClient iClient, string iGameID, string iScore)
            : base(iClient)
        {
            GameID = iGameID;
            var url = MakeURL(Client.Session, "player", "set_state");
            var form = new WWWForm();

            form.AddField("game", iGameID);
            form.AddField("value", iScore);

            Request = new WWW(url, form);
        }

        protected override bool GetResult()
        {
            var response = JsonMapper.ToObject(Request.text);

            if (IsError(response))
            {
                return false;
            }

            return true;
        }
    }

    public class GetWalletProcedure : UGSRequestProcedure<string>
    {
        //public readonly 
        internal GetWalletProcedure(UgsClient iClient) :
            base(iClient)
        {
            var url = MakeURL(Client.Session, "player", "wallet");
            Request = new WWW(url);
        }

        protected override string GetResult()
        {
            var response = JsonMapper.ToObject(Request.text);

            if (IsError(response))
            {
                return "";
            }

            return response[1][0].AsString();
        }

    }

    public class GetStateProcedure : UGSRequestProcedure<string>
    {
        public readonly string GameID;

        internal GetStateProcedure(UgsClient iClient, string iGameId)
            : base(iClient)
        {
            GameID = iGameId;
            var url = MakeURL(Client.Session, "player", "get_state");
            var form = new WWWForm();

            form.AddField("game", iGameId);

            Request = new WWW(url, form);
        }

        protected override string GetResult()
        {
            var response = JsonMapper.ToObject(Request.text);

            if (IsError(response))
            {
                return "";
            }

            return response[1][0].AsString();
        }
    }

    public class LoadScoreProcedure : UGSRequestProcedure<string>
    {
        public readonly string GameId;

        internal LoadScoreProcedure(UgsClient iClient, string iGameId)
            : base(iClient)
        {
            GameId = iGameId;
            var url = MakeURL(Client.Session, "player", "load_score");
            var form = new WWWForm();

            form.AddField("game", iGameId);

            Request = new WWW(url, form);
        }

        protected override string GetResult()
        {
            /*var response = JsonMapper.ToObject(Request.text);

            if (IsError(response))
            {
                return "";
            }*/

            return Request.text;
        }
    }

    public class SaveScoreProcedure : UGSRequestProcedure<bool>
    {
        public readonly string GameID;

        internal SaveScoreProcedure(UgsClient iClient, string iGameID, int iScore)
            : base(iClient)
        {
            GameID = iGameID;
            var url = MakeURL(Client.Session, "player", "save_score");
            var form = new WWWForm();

            form.AddField("game", iGameID);
            form.AddField("value", iScore);

            Request = new WWW(url, form);
        }

        protected override bool GetResult()
        {
            var response = JsonMapper.ToObject(Request.text);

            if (IsError(response))
            {
                return false;
            }

            return true; //response[1][0].AsInt();
        }
    }

    public class LessonCommitProcedure : UGSRequestProcedure<int>
    {
        public readonly string LessonId;

        internal LessonCommitProcedure(UgsClient client, string lessonId, bool lf)
            : base(client)
        {
            LessonId = lessonId;
            var url = lf?
                MakeURL(Client.Session, "lessonfile", LessonId, "commit"):
                MakeURL(Client.Session, "lesson", LessonId, "commit");
            Request = new WWW(url);
        }

        protected override int GetResult()
        {
            var response = JsonMapper.ToObject(Request.text);

            if (IsError(response))
            {
                throw new NotImplementedException();
            }

            return response[1][0].AsInt();
        }
    }

    public class LessonSwitchProcedure : UGSRequestProcedure<int>
    {
        public readonly string LessonId;
        private readonly int _number;

        internal LessonSwitchProcedure(UgsClient client, string lessonId, int number, bool lf)
            : base(client)
        {
            LessonId = lessonId;
            _number = number;
            var url = lf?
                MakeURL(Client.Session, "lessonfile", LessonId, "switch", number.ToString(CultureInfo.InvariantCulture)):
                MakeURL(Client.Session, "lesson", LessonId, "switch", number.ToString(CultureInfo.InvariantCulture));
            Request = new WWW(url);
        }

        protected override int GetResult()
        {
            var response = JsonMapper.ToObject(Request.text);

            if (IsError(response))
            {
                throw new NotImplementedException();
            }

            return _number;
        }
    }

    public class FileUploadProcedure : RequestProcedure<WWW>
    {
        protected readonly UploadPolicy Policy;

        public FileUploadProcedure(UploadPolicy policy, byte[] content)
        {
            Policy = policy;

            var url = string.Format("http://{0}.s3.amazonaws.com/", Policy.Bucket);

            var form = new WWWForm();
            form.AddField("AWSAccessKeyId", Policy.AccessKey);
            form.AddField("acl", "public-read");
            form.AddField("policy", Policy.Policy);
            form.AddField("Signature", Policy.Signature);
            form.AddField("key", Policy.ObjectKey);
            form.AddBinaryData("file", content);

            Request = new WWW(url, form);
        }

        protected override WWW GetResult()
        {
            return Request;
        }
    }

    public class FileDownloadProcedure : RequestProcedure<byte[]>
    {
        public FileDownloadProcedure(FileMetadata meta)
        {
            var url = string.Format("http://{0}.s3.amazonaws.com/{1}?versionId={2}",
                meta.Bucket, meta.Key, meta.Version);

            Request = new WWW(url);
        }

        protected override byte[] GetResult()
        {
            return Request.bytes;
        }
    }

    public struct UploadPolicy
    {
        public string Policy;
        public string Signature;
        public string ObjectKey;
        public string AccessKey;
        public string Bucket;

        public FileUploadProcedure Upload(byte[] content)
        {
            return new FileUploadProcedure(this, content);
        }
    }

    public class UploadAvatarProcedure : UGSRequestProcedure<WWW>
    {
        private byte[] png;
        protected readonly string Url;
        protected WWWForm Form;

        public UploadAvatarProcedure(UgsClient client, byte[] iPng) : base(client)
        {
            png = iPng;
            Url = MakeURL(Client.Session, "player", "upload_avatar");
            //LessonId = lessonId;
            Form = new WWWForm();
            Form.AddField("avatar", "avatar.png");
            Form.AddBinaryData("avatar", png);

            // Request = new WWW(Url, Form);
        }

        public override IEnumerator Process()
        {
            Request = new WWW(Url, Form);
            return base.Process();
        }

        protected override WWW GetResult()
        {
            return Result;
        }
    }

	public class DownloadAvatarProcedure : UGSRequestProcedure<string>
	{
		public Texture2D Avatar
		{
			get;
			private set;
		}

		public DownloadAvatarProcedure(UgsClient client, string username):base(client)
		{
			Avatar = null;
			string url =  MakeURL("player", username, "load_avatar");
			Request = new WWW(url);
			Done += InitAvatar;
		}

		protected override string GetResult()
		{
			return Request.text;
		}
		
		private void InitAvatar(string result)
		{
			Done -= InitAvatar;
			if (!HasError)
			{
				Texture2D tex = new Texture2D(512, 512);
				tex.LoadImage(Convert.FromBase64String(JsonMapper.ToObject(result)[2]["avatar"].AsString()));
                
				tex.Apply();
				Avatar = tex;
			}
		}
	}

    public class CheckoutProcedure : UGSRequestProcedure<Dictionary<string, UploadPolicy>>
    {
        public readonly string LessonId;
        protected readonly string Url;
        protected WWWForm Form;

        public CheckoutProcedure(UgsClient client, string lessonId, bool lf)
            : base(client)
        {
            Url = lf?
                MakeURL(Client.Session, "lessonfile", lessonId, "checkout"):
                MakeURL(Client.Session, "lesson", lessonId, "checkout");
            LessonId = lessonId;
            Form = new WWWForm();
        }

        public void AddFile(string itemId, int size, string name)
        {
            Form.AddField(string.Format("files.{0}.name", itemId), name);
            Form.AddField(string.Format("files.{0}.size", itemId), size);
        }

        public void AddMeta(string name, string value)
        {
            Form.AddField(string.Format("meta.{0}", name), value);
        }

        public override IEnumerator Process()
        {
            Request = new WWW(Url, Form);
            return base.Process();
        }

        protected override Dictionary<string, UploadPolicy> GetResult()
        {
            var response = JsonMapper.ToObject(Request.text);

            if (IsError(response))
            {
                throw new NotImplementedException();
            }

            var policies = new Dictionary<string, UploadPolicy>();

            foreach (DictionaryEntry entry in (IDictionary) response[1][0])
            {
                var value = (JsonData) entry.Value;
                policies[(string) entry.Key] = new UploadPolicy
                {
                    Policy = value["policy"].AsString(),
                    Signature = value["signature"].AsString(),
                    ObjectKey = value["object_key"].AsString(),
                    AccessKey = value["access_key"].AsString(),
                    Bucket = value["bucket"].AsString(),
                };
            }

            return policies;
        }
    }


    

    public class UgsClient
    {
        public readonly string URL;

        public UgsClient(string url)
        {
            URL = url.TrimEnd('/');
        }

        internal bool Logged;
        public string Session { get; internal set; }
        public string UserLogin { get; internal set; }

        public Auth_PIN Auth_pin(string mac_adress)
        {
            return new Auth_PIN(this,mac_adress);
        }

        public LoginProcedure Login(string login, string password)
        {
            return new LoginProcedure(this, login, password);
        }

		public void LoginBySession(string login, string session)
		{
			Logged = true;
			UserLogin = login;
			Session = session;
		}

       

       
    }

    public class GetExperienceCategoriesProcedure: UGSRequestProcedure<string>
    {
        public GetExperienceCategoriesProcedure(UgsClient ugsClient): base(ugsClient)
        {
            var url = MakeURL("gamedata", "experience");
            Request = new WWW(url);
        }

        protected override string GetResult()
        {
            var response = Request.text;
            Debug.Log("Response:" + response);
            return response;
        }
    }

    public class ScorePolicyProcedure : UGSRequestProcedure<string>
    {
       
        public ScorePolicyProcedure(UgsClient ugsClient, string gamename) : base(ugsClient)
        {
            var url = MakeURL("gamedata", "score_policy");
            var form = new WWWForm();
            form.AddField("game", gamename);
            Request = new WWW(url, form);
        }

        protected override string GetResult()
        {
            var response = Request.text;
            Debug.Log("Response:" + response);
            return response;
        }
    }

    public class LoadProfileProcedure : UGSRequestProcedure<string>
    {
        public LoadProfileProcedure(UgsClient ugsClient):
            base(ugsClient)
        {
            var url = MakeURL(ugsClient.Session, "player", "load_profile");
            Request = new WWW(url);
            Debug.Log("URL:" + url);
        }

        protected override string GetResult()
        {
            var response = Request.text;
            Debug.Log("Response:" + response);
            return response;
        }
    }

    public class PublicProfileProcedure : UGSRequestProcedure<string>
    {
        //private string _login = null;

        public PublicProfileProcedure(UgsClient client, string login) : base(client)
        {
           // this._login = login;
            var url = MakeURL("player",login,"public_profile");
            Debug.Log("URL:"+url);
            Request = new WWW(url);
        }

        protected override string GetResult()
        {
            var response = Request.text;
            Debug.Log("Response:" + response);
            return response;
        }
    }

    public class PurchaseProcedure : UGSRequestProcedure<bool>
    {
        public PurchaseProcedure(UgsClient client, string iOid) : base(client)
        {
            var url = MakeURL(client.Session, "player", "purchase");

            var form = new WWWForm();
            form.AddField("oid", iOid);

            Request = new WWW(url, form);
            Debug.Log("URL:" + url);
        }

        protected override bool GetResult()
        {
            var response = JsonMapper.ToObject(Request.text);
            //var response = Request.text;
            Debug.Log("Response:"+response);

            if (IsError(response))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}