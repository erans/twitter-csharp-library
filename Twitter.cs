//
// Yedda Twitter C# Library (or more of an API wrapper) v0.2
// Written by Eran Sandler (eran AT yedda.com)
// http://devblog.yedda.com/index.php/twitter-c-library/
// http://github.com/erans/twitter-csharp-library
//
// The library is provided on a "AS IS" basis. Yedda is not repsonsible in any way 
// for whatever usage you do with it.
//
// Giving credit would be nice though :-)
//
// Get more cool dev information and other stuff at the Yedda Dev Blog:
// http://devblog.yedda.com
//
// Got a question about this library? About programming? C#? .NET? About anything else?
// Ask about it at Yedda (http://yedda.com) and get answers from real people.
//
using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Web;
using System.Collections.Generic;
using System.Text;

namespace Yedda {
	public class Twitter {

		/// <summary>
		/// The output formats supported by Twitter. Not all of them can be used with all of the functions.
		/// For more information about the output formats and the supported functions Check the 
		/// Twitter documentation at: http://apiwiki.twitter.com/REST+API+Documentation
		/// </summary>
		public enum OutputFormatType {
			JSON,
			XML,
			RSS,
			Atom
		}

		/// <summary>
		/// The various object types supported at Twitter.
		/// </summary>
		public enum ObjectType {
			Statuses,
			Account,
			Users,
			Direct_Messages,
			Friendships,
			Favorites,
			Notifications,
			Blocks
		}

		/// <summary>
		/// The various actions used at Twitter. Not all actions works on all object types.
		/// For more information about the actions types and the supported functions Check the 
		/// Twitter documentation at: http://apiwiki.twitter.com/REST+API+Documentation
		/// </summary>
		public enum ActionType {
			Public_Timeline,
			User_Timeline,
			Friends_Timeline,
			Friends,
			Followers,
			Update,
			Account_Settings,
			Featured,
			Show,
			Replies,
			Destroy,
			Sent,
			New,
			Create,
			Exists,
			Verify_Credentials,
			End_Session,
			Update_Delivery_Device,
			Update_Profile_Colors,
			Rate_Limit_Status,
			Update_Profile,
			Follow,
			Leave
		}

		private string source = null;

		private string twitterClient = null;
		private string twitterClientVersion = null;
		private string twitterClientUrl = null;


		/// <summary>
		/// Source is an additional parameters that will be used to fill the "From" field.
		/// Currently you must talk to the developers of Twitter at:
		/// http://groups.google.com/group/twitter-development-talk/
		/// Otherwise, Twitter will simply ignore this parameter and set the "From" field to "web".
		/// </summary>
		public string Source {
			get { return source; }
			set { source = value; }
		}

		/// <summary>
		/// Sets the name of the Twitter client.
		/// According to the Twitter Fan Wiki at http://twitter.pbwiki.com/API-Docs and supported by
		/// the Twitter developers, this will be used in the future (hopefully near) to set more information
		/// in Twitter about the client posting the information as well as future usage in a clients directory.
		/// </summary>
		public string TwitterClient {
			get { return twitterClient; }
			set { twitterClient = value; }
		}

		/// <summary>
		/// Sets the version of the Twitter client.
		/// According to the Twitter Fan Wiki at http://twitter.pbwiki.com/API-Docs and supported by
		/// the Twitter developers, this will be used in the future (hopefully near) to set more information
		/// in Twitter about the client posting the information as well as future usage in a clients directory.
		/// </summary>
		public string TwitterClientVersion {
			get { return twitterClientVersion; }
			set { twitterClientVersion = value; }
		}

		/// <summary>
		/// Sets the URL of the Twitter client.
		/// Must be in the XML format documented in the "Request Headers" section at:
		/// http://twitter.pbwiki.com/API-Docs.
		/// According to the Twitter Fan Wiki at http://twitter.pbwiki.com/API-Docs and supported by
		/// the Twitter developers, this will be used in the future (hopefully near) to set more information
		/// in Twitter about the client posting the information as well as future usage in a clients directory.		
		/// </summary>
		public string TwitterClientUrl {
			get { return twitterClientUrl; }
			set { twitterClientUrl = value; }
		}

		protected const string TwitterUrl = "http://twitter.com/";

		protected const string TwitterBaseUrlFormat = TwitterUrl + "{0}/{1}.{2}";

		protected string GetObjectTypeString(ObjectType objectType) {
			return objectType.ToString().ToLower();
		}

		protected string GetActionTypeString(ActionType actionType) {
			return actionType.ToString().ToLower();
		}

		protected string GetFormatTypeString(OutputFormatType format) {
			return format.ToString().ToLower();
		}

		/// <summary>
		/// Executes an HTTP GET command and retrives the information.		
		/// </summary>
		/// <param name="url">The URL to perform the GET operation</param>
		/// <param name="userName">The username to use with the request</param>
		/// <param name="password">The password to use with the request</param>
		/// <returns>The response of the request, or null if we got 404 or nothing.</returns>
		protected string ExecuteGetCommand(string url, string userName, string password) {
			System.Net.ServicePointManager.Expect100Continue = false;

			using (WebClient client = new WebClient()) {
				if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password)) {
					client.Credentials = new NetworkCredential(userName, password);
				}

				try {
					using (Stream stream = client.OpenRead(url)) {
						using (StreamReader reader = new StreamReader(stream)) {
							return reader.ReadToEnd();
						}
					}
				}
				catch (WebException ex) {
					//
					// Handle HTTP 404 errors gracefully and return a null string to indicate there is no content.
					//
					if (ex.Response is HttpWebResponse) {
						if ((ex.Response as HttpWebResponse).StatusCode == HttpStatusCode.NotFound) {
							return null;
						}
					}

					throw ex;
				}
			}			
		}

		/// <summary>
		/// Executes an HTTP POST command and retrives the information.		
		/// This function will automatically include a "source" parameter if the "Source" property is set.
		/// </summary>
		/// <param name="url">The URL to perform the POST operation</param>
		/// <param name="userName">The username to use with the request</param>
		/// <param name="password">The password to use with the request</param>
		/// <param name="data">The data to post</param> 
		/// <returns>The response of the request, or null if we got 404 or nothing.</returns>
		protected string ExecutePostCommand(string url, string userName, string password, string data) {
			System.Net.ServicePointManager.Expect100Continue = false;

			WebRequest request = WebRequest.Create(url);
			if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password)) {
				request.Credentials = new NetworkCredential(userName, password);
				request.ContentType = "application/x-www-form-urlencoded";
				request.Method = "POST";

				if (!string.IsNullOrEmpty(TwitterClient)) {
					request.Headers.Add("X-Twitter-Client", TwitterClient);
				}

				if (!string.IsNullOrEmpty(TwitterClientVersion)) {
					request.Headers.Add("X-Twitter-Version", TwitterClientVersion);
				}

				if (!string.IsNullOrEmpty(TwitterClientUrl)) {
					request.Headers.Add("X-Twitter-URL", TwitterClientUrl);
				}


				if (!string.IsNullOrEmpty(Source)) {
					data += "&source=" + HttpUtility.UrlEncode(Source);
				}

				byte[] bytes = Encoding.UTF8.GetBytes(data);

				request.ContentLength = bytes.Length;
				using (Stream requestStream = request.GetRequestStream()) {
					requestStream.Write(bytes, 0, bytes.Length);

					using (WebResponse response = request.GetResponse()) {
						using (StreamReader reader = new StreamReader(response.GetResponseStream())) {
							return reader.ReadToEnd();
						}
					}
				}
			}

			return null;
		}

		#region Public_Timeline

		public string GetPublicTimeline(OutputFormatType format) {
			string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Statuses), GetActionTypeString(ActionType.Public_Timeline), GetFormatTypeString(format));
			return ExecuteGetCommand(url, null, null);
		}

		public string GetPublicTimelineAsJSON() {
			return GetPublicTimeline(OutputFormatType.JSON);
		}

		public XmlDocument GetPublicTimelineAsXML(OutputFormatType format) {
			if (format == OutputFormatType.JSON) {
				throw new ArgumentException("GetPublicTimelineAsXml supports only XML based formats (XML, RSS, Atom)", "format");
			}

			string output = GetPublicTimeline(format);
			if (!string.IsNullOrEmpty(output)) {
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(output);

				return xmlDocument;
			}

			return null;
		}

		public XmlDocument GetPublicTimelineAsXML() {
			return GetPublicTimelineAsXML(OutputFormatType.XML);
		}

		public XmlDocument GetPublicTimelineAsRSS() {
			return GetPublicTimelineAsXML(OutputFormatType.RSS);
		}

		public XmlDocument GetPublicTimelineAsAtom() {
			return GetPublicTimelineAsXML(OutputFormatType.Atom);
		}

		#endregion

		#region User_Timeline

		public string GetUserTimeline(string userName, string password, string IDorScreenName, OutputFormatType format) {
			string url = null;
			if (string.IsNullOrEmpty(IDorScreenName)) {
				url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Statuses), GetActionTypeString(ActionType.User_Timeline), GetFormatTypeString(format));
			}
			else {
				url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Statuses), GetActionTypeString(ActionType.User_Timeline) + "/" + IDorScreenName, GetFormatTypeString(format));
			}

			return ExecuteGetCommand(url, userName, password);
		}

		public string GetUserTimeline(string userName, string password, OutputFormatType format) {
			return GetUserTimeline(userName, password, null, format);
		}

		public string GetUserTimelineAsJSON(string userName, string password) {
			return GetUserTimeline(userName, password, OutputFormatType.JSON);
		}

		public string GetUserTimelineAsJSON(string userName, string password, string IDorScreenName) {
			return GetUserTimeline(userName, password, IDorScreenName, OutputFormatType.JSON);
		}

		public XmlDocument GetUserTimelineAsXML(string userName, string password, string IDorScreenName, OutputFormatType format) {
			if (format == OutputFormatType.JSON) {
				throw new ArgumentException("GetUserTimelineAsXML supports only XML based formats (XML, RSS, Atom)", "format");
			}

			string output = GetUserTimeline(userName, password, IDorScreenName, format);
			if (!string.IsNullOrEmpty(output)) {
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(output);

				return xmlDocument;
			}

			return null;
		}

		public XmlDocument GetUserTimelineAsXML(string userName, string password, OutputFormatType format) {
			return GetUserTimelineAsXML(userName, password, null, format);
		}

		public XmlDocument GetUserTimelineAsXML(string userName, string password, string IDorScreenName) {
			return GetUserTimelineAsXML(userName, password, IDorScreenName, OutputFormatType.XML);
		}

		public XmlDocument GetUserTimelineAsXML(string userName, string password) {
			return GetUserTimelineAsXML(userName, password, null);
		}

		public XmlDocument GetUserTimelineAsRSS(string userName, string password, string IDorScreenName) {
			return GetUserTimelineAsXML(userName, password, IDorScreenName, OutputFormatType.RSS);
		}

		public XmlDocument GetUserTimelineAsRSS(string userName, string password) {
			return GetUserTimelineAsXML(userName, password, OutputFormatType.RSS);
		}

		public XmlDocument GetUserTimelineAsAtom(string userName, string password, string IDorScreenName) {
			return GetUserTimelineAsXML(userName, password, IDorScreenName, OutputFormatType.Atom);
		}

		public XmlDocument GetUserTimelineAsAtom(string userName, string password) {
			return GetUserTimelineAsXML(userName, password, OutputFormatType.Atom);
		}
		#endregion

		#region Friends_Timeline
		public string GetFriendsTimeline(string userName, string password, OutputFormatType format) {
			string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Statuses), GetActionTypeString(ActionType.Friends_Timeline), GetFormatTypeString(format));

			return ExecuteGetCommand(url, userName, password);
		}

		public string GetFriendsTimelineAsJSON(string userName, string password) {
			return GetFriendsTimeline(userName, password, OutputFormatType.JSON);
		}

		public XmlDocument GetFriendsTimelineAsXML(string userName, string password, OutputFormatType format) {
			if (format == OutputFormatType.JSON) {
				throw new ArgumentException("GetFriendsTimelineAsXML supports only XML based formats (XML, RSS, Atom)", "format");
			}

			string output = GetFriendsTimeline(userName, password, format);
			if (!string.IsNullOrEmpty(output)) {
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(output);

				return xmlDocument;
			}

			return null;
		}

		public XmlDocument GetFriendsTimelineAsXML(string userName, string password) {
			return GetFriendsTimelineAsXML(userName, password, OutputFormatType.XML);
		}

		public XmlDocument GetFriendsTimelineAsRSS(string userName, string password) {
			return GetFriendsTimelineAsXML(userName, password, OutputFormatType.RSS);
		}

		public XmlDocument GetFriendsTimelineAsAtom(string userName, string password) {
			return GetFriendsTimelineAsXML(userName, password, OutputFormatType.Atom);
		}

		#endregion

		#region Friends

		public string GetFriends(string userName, string password, OutputFormatType format) {
			if (format != OutputFormatType.JSON && format != OutputFormatType.XML) {
				throw new ArgumentException("GetFriends support only XML and JSON output format", "format");
			}

			string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Statuses), GetActionTypeString(ActionType.Friends), GetFormatTypeString(format));
			return ExecuteGetCommand(url, userName, password);
		}

		public string GetFriends(string userName, string password, string IDorScreenName, OutputFormatType format) {
			if (format != OutputFormatType.JSON && format != OutputFormatType.XML) {
				throw new ArgumentException("GetFriends support only XML and JSON output format", "format");
			}

			string url = null;
			if (string.IsNullOrEmpty(IDorScreenName)) {
				url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Statuses), GetActionTypeString(ActionType.Friends), GetFormatTypeString(format));
			}
			else {
				url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Statuses), GetActionTypeString(ActionType.Friends) + "/" + IDorScreenName, GetFormatTypeString(format));
			}

			return ExecuteGetCommand(url, userName, password);
		}

		public string GetFriendsAsJSON(string userName, string password, string IDorScreenName) {
			return GetFriends(userName, password, IDorScreenName, OutputFormatType.JSON);
		}

		public string GetFriendsAsJSON(string userName, string password) {
			return GetFriendsAsJSON(userName, password, null);
		}

		public XmlDocument GetFriendsAsXML(string userName, string password, string IDorScreenName) {
			string output = GetFriends(userName, password, IDorScreenName, OutputFormatType.XML);
			if (!string.IsNullOrEmpty(output)) {
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(output);

				return xmlDocument;
			}

			return null;
		}

		public XmlDocument GetFriendsAsXML(string userName, string password) {
			return GetFriendsAsXML(userName, password, null);
		}

		#endregion

		#region Followers

		public string GetFollowers(string userName, string password, OutputFormatType format) {
			if (format != OutputFormatType.JSON && format != OutputFormatType.XML) {
				throw new ArgumentException("GetFollowers supports only XML and JSON output format", "format");
			}

			string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Statuses), GetActionTypeString(ActionType.Followers), GetFormatTypeString(format));
			return ExecuteGetCommand(url, userName, password);
		}

		public string GetFollowersAsJSON(string userName, string password) {
			return GetFollowers(userName, password, OutputFormatType.JSON);
		}

		public XmlDocument GetFollowersAsXML(string userName, string password) {
			string output = GetFollowers(userName, password, OutputFormatType.XML);
			if (!string.IsNullOrEmpty(output)) {
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(output);

				return xmlDocument;
			}

			return null;
		}

		#endregion

		#region Update

		public string Update(string userName, string password, string status, OutputFormatType format) {
			if (format != OutputFormatType.JSON && format != OutputFormatType.XML) {
				throw new ArgumentException("Update support only XML and JSON output format", "format");
			}

			string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Statuses), GetActionTypeString(ActionType.Update), GetFormatTypeString(format));
			string data = string.Format("status={0}", HttpUtility.UrlEncode(status));

			return ExecutePostCommand(url, userName, password, data);
		}

		public string UpdateAsJSON(string userName, string password, string text) {
			return Update(userName, password, text, OutputFormatType.JSON);
		}

		public XmlDocument UpdateAsXML(string userName, string password, string text) {
			string output = Update(userName, password, text, OutputFormatType.XML);
			if (!string.IsNullOrEmpty(output)) {
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(output);

				return xmlDocument;
			}

			return null;
		}

		#endregion

		#region Featured

		public string GetFeatured(string userName, string password, OutputFormatType format) {
			if (format != OutputFormatType.JSON && format != OutputFormatType.XML) {
				throw new ArgumentException("GetFeatured supports only XML and JSON output format", "format");
			}

			string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Statuses), GetActionTypeString(ActionType.Featured), GetFormatTypeString(format));
			return ExecuteGetCommand(url, userName, password);
		}

		public string GetFeaturedAsJSON(string userName, string password) {
			return GetFeatured(userName, password, OutputFormatType.JSON);
		}

		public XmlDocument GetFeaturedAsXML(string userName, string password) {
			string output = GetFeatured(userName, password, OutputFormatType.XML);
			if (!string.IsNullOrEmpty(output)) {
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(output);

				return xmlDocument;
			}

			return null;
		}

		#endregion

		#region Show

		public string Show(string userName, string password, string IDorScreenName, OutputFormatType format) {
			if (format != OutputFormatType.JSON && format != OutputFormatType.XML) {
				throw new ArgumentException("Show supports only XML and JSON output format", "format");
			}

			string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Users), GetActionTypeString(ActionType.Show) + "/" + IDorScreenName, GetFormatTypeString(format));
			return ExecuteGetCommand(url, userName, password);
		}

		public string ShowAsJSON(string userName, string password, string IDorScreenName) {
			return Show(userName, password, IDorScreenName, OutputFormatType.JSON);
		}

		public XmlDocument ShowAsXML(string userName, string password, string IDorScreenName) {
			string output = Show(userName, password, IDorScreenName, OutputFormatType.XML);
			if (!string.IsNullOrEmpty(output)) {
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(output);

				return xmlDocument;
			}

			return null;
		}

		#endregion

		#region Replies

		public string Replies(string userName, string password, int? page, DateTime since, string since_id, OutputFormatType format) {
			StringBuilder url = new StringBuilder(string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Statuses), GetActionTypeString(ActionType.Replies), GetFormatTypeString(format)) + "?");
			if (page != null) {
				url.AppendFormat("page={0}&", page);
			}
			if (since != null) {
				url.AppendFormat("since={0}&", HttpUtility.UrlEncode(since.ToString("r")));
			}
			if (since_id != null) {
				url.AppendFormat("since_id={0}&", since_id);
			}

			return ExecuteGetCommand(url.ToString(), userName, password);
		}

		public string RepliesAsJSON(string userName, string password, int? page, DateTime since, string since_id) {
			return Replies(userName, password, page, since, since_id, OutputFormatType.JSON);
		}

		public XmlDocument RepliesAsXML(string userName, string password, int? page, DateTime since, string since_id, OutputFormatType format) {
			if (format != OutputFormatType.XML && format != OutputFormatType.Atom && format != OutputFormatType.RSS) {
				throw new ArgumentException("Replies supports only XML, Atom and RSS output formats", "format");
			}

			string output = Replies(userName, password, page, since, since_id, OutputFormatType.XML);
			if (!string.IsNullOrEmpty(output)) {
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(output);

				return xmlDocument;
			}

			return null;
		}

		public XmlDocument RepliesAsXML(string userName, string password, int? page, DateTime since, string since_id) {
			return RepliesAsXML(userName, password, page, since, since_id, OutputFormatType.XML);
		}

		public XmlDocument RepliesAsRSS(string userName, string password, int? page, DateTime since, string since_id) {
			return RepliesAsXML(userName, password, page, since, since_id, OutputFormatType.RSS);
		}

		public XmlDocument RepliesAsAtom(string userName, string password, int? page, DateTime since, string since_id) {
			return RepliesAsXML(userName, password, page, since, since_id, OutputFormatType.Atom);
		}

		#endregion

		#region Destory 

		public string Destroy(string userName, string password, string id, OutputFormatType format) {
			if (string.IsNullOrEmpty(id)) {
				throw new ArgumentNullException("id");
			}
			
			string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Statuses), GetActionTypeString(ActionType.Destroy) + "/" + id, GetFormatTypeString(format));
			return ExecutePostCommand(url, userName, password, null);
		}

		public string DestroyAsJSON(string userName, string password, string id) {
			return Destroy(userName, password, id, OutputFormatType.JSON);
		}

		public XmlDocument DestroyAsXML(string userName, string password, string id) {
			string output = Destroy(userName, password, id, OutputFormatType.XML);
			if (!string.IsNullOrEmpty(output)) {
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(output);

				return xmlDocument;
			}

			return null;
		}

		#endregion

		#region direct_messages

		public string DirectMessages(string userName, string password, DateTime since, string since_id, int? page, OutputFormatType format) {
			StringBuilder url = new StringBuilder(string.Format("{0}{1}.{2}?", TwitterUrl, GetObjectTypeString(ObjectType.Direct_Messages), GetFormatTypeString(format)));

			if (since != null) {
				url.AppendFormat("since={0}&", HttpUtility.UrlEncode(since.ToString("r")));
			}
			if (!string.IsNullOrEmpty(since_id)) {
				url.AppendFormat("since_id={0}&", since_id);
			}
			if (page != null) {
				url.AppendFormat("page={0}", page);
			}

			return ExecuteGetCommand(url.ToString(), userName, password);
		}

		public string DirectMessagesAsJSON(string userName, string password, DateTime since, string since_id, int? page) {
			return DirectMessages(userName, password, since, since_id, page, OutputFormatType.JSON);			
		}

		public XmlDocument DirectMessagesAsXML(string userName, string password, DateTime since, string since_id, int? page, OutputFormatType format) {
			if (format != OutputFormatType.XML && format != OutputFormatType.Atom && format != OutputFormatType.RSS) {
				throw new ArgumentException("Direct_Messages supports only XML, Atom and RSS output formats", "format");
			}

			string output = DirectMessages(userName, password, since, since_id, page, OutputFormatType.XML);
			if (!string.IsNullOrEmpty(output)) {
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(output);

				return xmlDocument;
			}

			return null;
		}

		public XmlDocument DirectMessagesAsXML(string userName, string password, DateTime since, string since_id, int? page) {
			return DirectMessagesAsXML(userName, password, since, since_id, page, OutputFormatType.XML);
		}

		public XmlDocument DirectMessagesAsRSS(string userName, string password, DateTime since, string since_id, int? page) {
			return DirectMessagesAsXML(userName, password, since, since_id, page, OutputFormatType.RSS);
		}

		public XmlDocument DirectMessagesAsAtom(string userName, string password, DateTime since, string since_id, int? page) {
			return DirectMessagesAsXML(userName, password, since, since_id, page, OutputFormatType.Atom);
		}

		#endregion

		#region direct_messages sent

		public string DirectMessagesSent(string userName, string password, DateTime since, string since_id, int? page, OutputFormatType format) {
			StringBuilder url = new StringBuilder(string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Direct_Messages), GetActionTypeString(ActionType.Sent), GetFormatTypeString(format)) + "?");

			if (since != null) {
				url.AppendFormat("since={0}&", HttpUtility.UrlEncode(since.ToString("r")));
			}
			if (!string.IsNullOrEmpty(since_id)) {
				url.AppendFormat("since_id={0}&", since_id);
			}
			if (page != null) {
				url.AppendFormat("page={0}", page);
			}

			return ExecuteGetCommand(url.ToString(), userName, password);
		}

		public string DirectMessagesSentAsJSON(string userName, string password, DateTime since, string since_id, int? page) {
			return DirectMessagesSent(userName, password, since, since_id, page, OutputFormatType.JSON);
		}

		public XmlDocument DirectMessagesSentAsXML(string userName, string password, DateTime since, string since_id, int? page) {
			string output = DirectMessagesSent(userName, password, since, since_id, page, OutputFormatType.XML);
			if (!string.IsNullOrEmpty(output)) {
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(output);

				return xmlDocument;
			}

			return null;
		}
		
		#endregion

		#region direct_messages new 

		public string DirectMessagesNew(string userName, string password, string IDorScreenName, string text, OutputFormatType format) {
			if (string.IsNullOrEmpty(IDorScreenName)) {
				throw new ArgumentNullException("IDorScreenName");
			}

			if (string.IsNullOrEmpty(text)) {
				throw new ArgumentNullException("text");
			}
			
			string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Direct_Messages), GetActionTypeString(ActionType.New), GetFormatTypeString(format));
			string data = string.Format("user={0}&text={1}", HttpUtility.UrlEncode(IDorScreenName), HttpUtility.UrlEncode(text));

			return ExecutePostCommand(url, userName, password, data);
		}

		public string DirectMessagesNewAsJSON(string userName, string password, string IDorScreenName, string text) {
			return DirectMessagesNew(userName, password, IDorScreenName, text, OutputFormatType.JSON);
		}

		public XmlDocument DirectMessagesNewAsXML(string userName, string password, string IDorScreenName, string text) {
			string output = DirectMessagesNew(userName, password, IDorScreenName, text, OutputFormatType.XML);
			if (!string.IsNullOrEmpty(output)) {
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(output);

				return xmlDocument;
			}

			return null;
		}

		#endregion

		#region direct_messages destroy

		public string DirectMessagesDestroy(string userName, string password, string id, OutputFormatType format) {
			if (string.IsNullOrEmpty(id)) {
				throw new ArgumentNullException("id");
			}
			
			string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Direct_Messages), GetActionTypeString(ActionType.Destroy), GetFormatTypeString(format));
			return ExecutePostCommand(url, userName, password, null);
		}

		public string DirectMessagesDestroyAsJSON(string userName, string password, string id) {
			return DirectMessagesDestroy(userName, password, id, OutputFormatType.JSON);
		}

		public XmlDocument DirectMessagesDestroyAsXML(string userName, string password, string id) {
			string output = DirectMessagesDestroy(userName, password, id, OutputFormatType.XML);
			if (!string.IsNullOrEmpty(output)) {
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(output);

				return xmlDocument;
			}

			return null;
		}

		#endregion

		#region Friendship create

		public string FriendshipCreate(string userName, string password, string IDorScreenName, bool? follow, OutputFormatType format) {
			if (string.IsNullOrEmpty(IDorScreenName)) {
				throw new ArgumentNullException("IDorScreenName");
			}

			StringBuilder url = new StringBuilder(string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Friendships), GetActionTypeString(ActionType.Create) + "/" + IDorScreenName + "/", GetFormatTypeString(format)) + "?");			
			if (follow != null) {				
				url.AppendFormat("follow={0}", (follow.Value ? "true" : "false"));
			}

			return ExecutePostCommand(url.ToString(), userName, password, null);
		}

		public string FriendshipCreateAsJSON(string userName, string password, string IDorScreenName, bool? follow) {
			return FriendshipCreate(userName, password, IDorScreenName, follow, OutputFormatType.JSON);
		}

		public XmlDocument FriendshipCreateAsXML(string userName, string password, string IDorScreenName, bool? follow) {
			string output = FriendshipCreate(userName, password, IDorScreenName, follow, OutputFormatType.XML);
			if (!string.IsNullOrEmpty(output)) {
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(output);

				return xmlDocument;
			}

			return null;
		}

		#endregion

		#region friendship destory

		public string FriendshipDestroy(string userName, string password, string IDorScreenName, OutputFormatType format) {
			if (string.IsNullOrEmpty(IDorScreenName)) {
				throw new ArgumentNullException("IDorScreenName");
			}
			
			string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Friendships), GetActionTypeString(ActionType.Destroy) + "/" + IDorScreenName + "/", GetFormatTypeString(format));
			return ExecutePostCommand(url, userName, password, null);
		}

		public string FriendshipDestroyAsJSON(string userName, string password, string IDorScreenName, OutputFormatType format) {
			return FriendshipDestroy(userName, password, IDorScreenName, OutputFormatType.JSON);
		}

		public XmlDocument FriendshipDestroyAsXML(string userName, string password, string IDorScreenName, OutputFormatType format) {
			string output = FriendshipDestroy(userName, password, IDorScreenName, OutputFormatType.XML);
			if (!string.IsNullOrEmpty(output)) {
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(output);

				return xmlDocument;
			}

			return null;
		}

		#endregion

		#region friendship exists

		public string FriendshipExists(string userName, string password, string user_a_IDorScreenName, string user_b_IDorScreenName, OutputFormatType format) {
			if (string.IsNullOrEmpty(user_a_IDorScreenName)) {
				throw new ArgumentNullException("user_a_IDorScreenName");
			}

			if (string.IsNullOrEmpty(user_b_IDorScreenName)) {
				throw new ArgumentNullException("user_b_IDorScreenName");
			}

			StringBuilder url =  new StringBuilder(string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Friendships), GetActionTypeString(ActionType.Exists), GetFormatTypeString(format)));
			url.AppendFormat("?user_a={0}&user_b={1}", user_a_IDorScreenName, user_b_IDorScreenName);

			return ExecuteGetCommand(url.ToString(), userName, password);
		}

		#endregion

		#region verify_credentials

		public string VerifyCredentials(string userName, string password, OutputFormatType format) {
			if (format != OutputFormatType.JSON && format != OutputFormatType.XML) {
				throw new ArgumentException("Replies supports only XML and JSON output formats", "format");
			}

			string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Account), GetActionTypeString(ActionType.Verify_Credentials), GetFormatTypeString(format));
			return ExecuteGetCommand(url, userName, password);
		}

		public string VerifyCredentialsAsJSON(string userName, string password) {
			return VerifyCredentials(userName, password, OutputFormatType.JSON);
		}

		public XmlDocument VerifyCredentialsAsXML(string userName, string password) {
			string output = VerifyCredentials(userName, password, OutputFormatType.XML);
			if (!string.IsNullOrEmpty(output)) {
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(output);

				return xmlDocument;
			}

			return null;
		}

		#endregion

		#region end_session

		public string EndSession(string userName, string password, OutputFormatType format) {
			if (format != OutputFormatType.JSON && format != OutputFormatType.XML) {
				throw new ArgumentException("Replies supports only XML and JSON output formats", "format");
			}

			string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Account), GetActionTypeString(ActionType.End_Session), GetFormatTypeString(format));
			return ExecutePostCommand(url, userName, password, null);
		}

		public string EndSessionAsJSON(string userName, string password) {
			return EndSession(userName, password, OutputFormatType.JSON);
		}

		public XmlDocument EndSessionAsXML(string userName, string password) {
			string output = EndSession(userName, password, OutputFormatType.XML);
			if (!string.IsNullOrEmpty(output)) {
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(output);

				return xmlDocument;
			}

			return null;
		}

		#endregion

		#region update_delivery_device

		public string UpdateDeliveryDevice(string userName, string password, string device, OutputFormatType format) {
			if (format != OutputFormatType.JSON && format != OutputFormatType.XML) {
				throw new ArgumentException("Replies supports only XML and JSON output formats", "format");
			}

			if (string.IsNullOrEmpty(device)) {
				throw new ArgumentNullException("device");
			}

			device = device.ToLower().Trim();
			if (device != "none" && device != "im" && device != "sms") {
				throw new ArgumentException("device can only have the values: none,im,sms", "device");
			}

			string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Account), GetActionTypeString(ActionType.Update_Delivery_Device), GetFormatTypeString(format));
			url += string.Format("?device={0}", device);
			return ExecutePostCommand(url, userName, password, null);
		}

		public string UpdateDeliveryDeviceAsJSON(string userName, string password, string device) {
			return UpdateDeliveryDevice(userName, password, device, OutputFormatType.JSON);
		}

		public XmlDocument UpdateDeliveryDeviceAsXML(string userName, string password, string device) {
			string output = UpdateDeliveryDevice(userName, password, device, OutputFormatType.XML);
			if (!string.IsNullOrEmpty(output)) {
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(output);

				return xmlDocument;
			}

			return null;
		}

		#endregion

		#region update_profile_colors

		public string UpdateProfileColors(string userName, string password, string profileBackgroundColor, string profileTextColor, string profileLinkColor, string profileSidebarFillColor, string profileSidebarBorderColor, OutputFormatType format) {
			if (format != OutputFormatType.JSON && format != OutputFormatType.XML) {
				throw new ArgumentException("Replies supports only XML and JSON output formats", "format");
			}

			string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Account), GetActionTypeString(ActionType.Update_Delivery_Device), GetFormatTypeString(format));
			StringBuilder data = new StringBuilder();
			if (!string.IsNullOrEmpty(profileBackgroundColor)) {
				data.AppendFormat("profile_background_color={0}&", profileBackgroundColor);
			}
			if (!string.IsNullOrEmpty(profileTextColor)) {
				data.AppendFormat("profile_text_color={0}&", profileTextColor);
			}
			if (!string.IsNullOrEmpty(profileLinkColor)) {
				data.AppendFormat("profile_link_color={0}&", profileLinkColor);
			}
			if (!string.IsNullOrEmpty(profileSidebarFillColor)) {
				data.AppendFormat("profile_sidebar_fill_color={0}&", profileSidebarFillColor);
			}
			if (!string.IsNullOrEmpty(profileSidebarBorderColor)) {
				data.AppendFormat("profile_sidebar_border_color={0}&", profileSidebarBorderColor);
			}

			if (data[data.Length - 1] == '&') {
				data.Remove(data.Length - 1, 1);
			}

			return ExecutePostCommand(url, userName, password, data.ToString());
		}

		public string UpdateProfileColorsAsJSON(string userName, string password, string profileBackgroundColor, string profileTextColor, string profileLinkColor, string profileSidebarFillColor, string profileSidebarBorderColor) {
			return UpdateProfileColors(userName, password, profileBackgroundColor, profileTextColor, profileLinkColor, profileSidebarFillColor, profileSidebarBorderColor, OutputFormatType.JSON);
		}

		public XmlDocument UpdateProfileColorsAsXML(string userName, string password, string profileBackgroundColor, string profileTextColor, string profileLinkColor, string profileSidebarFillColor, string profileSidebarBorderColor) {
			string output = UpdateProfileColors(userName, password, profileBackgroundColor, profileTextColor, profileLinkColor, profileSidebarFillColor, profileSidebarBorderColor, OutputFormatType.XML);
			if (!string.IsNullOrEmpty(output)) {
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(output);

				return xmlDocument;
			}

			return null;
		}

		#endregion

		#region rate_limit_status

		public string RateLimitStatus(string userName, string password, OutputFormatType format) {
			if (format != OutputFormatType.JSON && format != OutputFormatType.XML) {
				throw new ArgumentException("Replies supports only XML and JSON output formats", "format");
			}

			string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Account), GetActionTypeString(ActionType.Rate_Limit_Status), GetFormatTypeString(format));
			return ExecuteGetCommand(url, userName, password);
		}

		public string RateLimitStatusAsJSON(string userName, string password) {
			return RateLimitStatus(userName, password, OutputFormatType.JSON);
		}

		public XmlDocument RateLimitStatusAsXML(string userName, string password) {
			string output = RateLimitStatus(userName, password, OutputFormatType.XML);
			if (!string.IsNullOrEmpty(output)) {
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(output);

				return xmlDocument;
			}

			return null;
		}

		#endregion

		#region update_profile

		public string UpdateProfile(string userName, string password, string name, string email, string url, string location, string description, OutputFormatType format) {
			if (format != OutputFormatType.JSON && format != OutputFormatType.XML) {
				throw new ArgumentException("Replies supports only XML and JSON output formats", "format");
			}

			string actionUrl = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Account), GetActionTypeString(ActionType.Update_Profile), GetFormatTypeString(format));

			StringBuilder data = new StringBuilder();
			if (!string.IsNullOrEmpty(name)) {
				data.AppendFormat("name={0}&", name);
			}
			if (!string.IsNullOrEmpty(email)) {
				data.AppendFormat("email={0}&", email);
			}
			if (!string.IsNullOrEmpty(url)) {
				data.AppendFormat("url={0}&", url);
			}
			if (!string.IsNullOrEmpty(location)) {
				data.AppendFormat("location={0}&", location);
			}
			if (!string.IsNullOrEmpty(description)) {
				data.AppendFormat("description={0}&", description);
			}

			if (data[data.Length - 1] == '&') {
				data.Remove(data.Length - 1, 1);
			}

			return ExecutePostCommand(actionUrl, userName, password, data.ToString());
		}

		public string UpdateProfileAsJSON(string userName, string password, string name, string email, string url, string location, string description) {
			return UpdateProfile(userName, password, name, email, url, location, description, OutputFormatType.JSON);
		}

		public XmlDocument UpdateProfileAsXML(string userName, string password, string name, string email, string url, string location, string description) {
			string output = UpdateProfile(userName, password, name, email, url, location, description, OutputFormatType.XML);
			if (!string.IsNullOrEmpty(output)) {
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(output);

				return xmlDocument;
			}

			return null;
		}

		#endregion

		#region favorites

		public string Favorites(string userName, string password, string IDorScreenName, int? page, OutputFormatType format) {
			StringBuilder url = new StringBuilder(string.Format("{0}{1}.{2}", TwitterUrl, GetObjectTypeString(ObjectType.Favorites), GetFormatTypeString(format)) + "?");
			if (!string.IsNullOrEmpty(IDorScreenName)) {
				url.AppendFormat("id={0}&", IDorScreenName);
			}
			if (page != null) {
				url.AppendFormat("page={0}", page);
			}

			return ExecuteGetCommand(url.ToString(), userName, password);
		}

		public string FavoritesAsJSON(string userName, string password, string IDorScreenName, int? page, OutputFormatType format) {
			return Favorites(userName, password, IDorScreenName, page, OutputFormatType.JSON);
		}

		public XmlDocument FavoritesAsXML(string userName, string password, string IDorScreenName, int? page, OutputFormatType format) {
			if (format != OutputFormatType.XML && format != OutputFormatType.RSS && format != OutputFormatType.Atom) {
				throw new ArgumentException("FavoritesAsXML supports only XML, RSS and Atom output formats", "format");
			}

			string output = Favorites(userName, password, IDorScreenName, page, format);
			if (!string.IsNullOrEmpty(output)) {
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(output);

				return xmlDocument;
			}

			return null;
		}

		public XmlDocument FavoritesAsXML(string userName, string password, string IDorScreenName, int? page) {
			return FavoritesAsXML(userName, password, IDorScreenName, page, OutputFormatType.XML);
		}

		public XmlDocument FavoritesAsRSS(string userName, string password, string IDorScreenName, int? page) {
			return FavoritesAsXML(userName, password, IDorScreenName, page, OutputFormatType.RSS);
		}

		public XmlDocument FavoritesAsAtom(string userName, string password, string IDorScreenName, int? page) {
			return FavoritesAsXML(userName, password, IDorScreenName, page, OutputFormatType.Atom);
		}

		#endregion

		#region favorites create

		public string FavoritesCreate(string userName, string password, string id, OutputFormatType format) {
			if (string.IsNullOrEmpty(id)) {
				throw new ArgumentNullException("id");
			}

			if (format != OutputFormatType.JSON && format != OutputFormatType.XML) {
				throw new ArgumentException("FavoritesCreate supports only XML and JSON output formats", "format");
			}			

			string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Favorites), GetActionTypeString(ActionType.Create) + "/" + id, GetFormatTypeString(format));
			return ExecutePostCommand(url, userName, password, null);
		}

		public string FavoritesCreateAsJSON(string userName, string password, string id) {
			return FavoritesCreate(userName, password, id, OutputFormatType.JSON);
		}

		public XmlDocument FavoritesCreateAsXML(string userName, string password, string id) {
			string output = FavoritesCreate(userName, password, id, OutputFormatType.XML);
			if (!string.IsNullOrEmpty(output)) {
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(output);

				return xmlDocument;
			}

			return null;			
		}

		#endregion

		#region favorites destroy

		public string FavoritesDestroy(string userName, string password, string id, OutputFormatType format) {
			if (string.IsNullOrEmpty(id)) {
				throw new ArgumentNullException("id");
			}

			if (format != OutputFormatType.JSON && format != OutputFormatType.XML) {
				throw new ArgumentException("FavoritesCreate supports only XML and JSON output formats", "format");
			}

			string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Favorites), GetActionTypeString(ActionType.Destroy) + "/" + id, GetFormatTypeString(format));
			return ExecutePostCommand(url, userName, password, null);
		}

		public string FavoritesDestroyAsJSON(string userName, string password, string id) {
			return FavoritesDestroy(userName, password, id, OutputFormatType.JSON);
		}

		public XmlDocument FavoritesDestroyAsXML(string userName, string password, string id) {
			string output = FavoritesDestroy(userName, password, id, OutputFormatType.XML);
			if (!string.IsNullOrEmpty(output)) {
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(output);

				return xmlDocument;
			}

			return null;
		}

		#endregion

		#region follow

		public string Follow(string userName, string password, string IDorScreenName, OutputFormatType format) {
			if (string.IsNullOrEmpty(IDorScreenName)) {
				throw new ArgumentNullException("IDorScreenName");
			}

			if (format != OutputFormatType.JSON && format != OutputFormatType.XML) {
				throw new ArgumentException("Follow supports only XML and JSON output formats", "format");
			}
			
			string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Notifications), GetActionTypeString(ActionType.Follow) + "/" + IDorScreenName, GetFormatTypeString(format));
			return ExecutePostCommand(url, userName, password, null);
		}

		public string FollowAsJSON(string userName, string password, string IDorScreenName, OutputFormatType format) {
			return Follow(userName, password, IDorScreenName, OutputFormatType.JSON);
		}

		public XmlDocument FollowAsXML(string userName, string password, string IDorScreenName, OutputFormatType format) {
			string output = Follow(userName, password, IDorScreenName, OutputFormatType.XML);
			if (!string.IsNullOrEmpty(output)) {
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(output);

				return xmlDocument;
			}

			return null;
		}

		#endregion

		#region leave

		public string Leave(string userName, string password, string IDorScreenName, OutputFormatType format) {
			if (string.IsNullOrEmpty(IDorScreenName)) {
				throw new ArgumentNullException("IDorScreenName");
			}

			if (format != OutputFormatType.JSON && format != OutputFormatType.XML) {
				throw new ArgumentException("Follow supports only XML and JSON output formats", "format");
			}

			string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Notifications), GetActionTypeString(ActionType.Leave) + "/" + IDorScreenName, GetFormatTypeString(format));
			return ExecutePostCommand(url, userName, password, null);
		}

		public string LeaveAsJSON(string userName, string password, string IDorScreenName, OutputFormatType format) {
			return Leave(userName, password, IDorScreenName, OutputFormatType.JSON);
		}

		public XmlDocument LeaveAsXML(string userName, string password, string IDorScreenName, OutputFormatType format) {
			string output = Leave(userName, password, IDorScreenName, OutputFormatType.XML);
			if (!string.IsNullOrEmpty(output)) {
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(output);

				return xmlDocument;
			}

			return null;
		}

		#endregion

		#region block create

		public string BlockCreate(string userName, string password, string IDorScreenName, OutputFormatType format) {
			if (string.IsNullOrEmpty(IDorScreenName)) {
				throw new ArgumentNullException("IDorScreenName");
			}

			if (format != OutputFormatType.JSON && format != OutputFormatType.XML) {
				throw new ArgumentException("Follow supports only XML and JSON output formats", "format");
			}

			string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Blocks), GetActionTypeString(ActionType.Create) + "/" + IDorScreenName, GetFormatTypeString(format));
			return ExecutePostCommand(url, userName, password, null);
		}

		public string BlockCreateAsJSON(string userName, string password, string IDorScreenName, OutputFormatType format) {
			return BlockCreate(userName, password, IDorScreenName, OutputFormatType.JSON);
		}

		public XmlDocument BlockCreateAsXML(string userName, string password, string IDorScreenName, OutputFormatType format) {
			string output = BlockCreate(userName, password, IDorScreenName, OutputFormatType.XML);
			if (!string.IsNullOrEmpty(output)) {
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(output);

				return xmlDocument;
			}

			return null;
		}

		#endregion

		#region block destroy

		public string BlockDestroy(string userName, string password, string IDorScreenName, OutputFormatType format) {
			if (string.IsNullOrEmpty(IDorScreenName)) {
				throw new ArgumentNullException("IDorScreenName");
			}

			if (format != OutputFormatType.JSON && format != OutputFormatType.XML) {
				throw new ArgumentException("Follow supports only XML and JSON output formats", "format");
			}

			string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Blocks), GetActionTypeString(ActionType.Destroy) + "/" + IDorScreenName, GetFormatTypeString(format));
			return ExecutePostCommand(url, userName, password, null);
		}

		public string BlockDestroyAsJSON(string userName, string password, string IDorScreenName, OutputFormatType format) {
			return BlockDestroy(userName, password, IDorScreenName, OutputFormatType.JSON);
		}

		public XmlDocument BlockDestroyAsXML(string userName, string password, string IDorScreenName, OutputFormatType format) {
			string output = BlockDestroy(userName, password, IDorScreenName, OutputFormatType.XML);
			if (!string.IsNullOrEmpty(output)) {
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(output);

				return xmlDocument;
			}

			return null;
		}

		#endregion
	}	
}

