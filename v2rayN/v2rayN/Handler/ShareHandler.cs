﻿using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Web;
using v2rayN.Base;
using v2rayN.Mode;
using v2rayN.Resx;

namespace v2rayN.Handler
{
    class ShareHandler
    {

        #region GetShareUrl

        /// <summary>
        /// GetShareUrl
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string GetShareUrl(ProfileItem item)
        {
            try
            {
                string url = string.Empty;

                switch (item.configType)
                {
                    case EConfigType.VMess:
                        url = ShareVmess(item);
                        break;
                    case EConfigType.Shadowsocks:
                        url = ShareShadowsocks(item);
                        break;
                    case EConfigType.Socks:
                        url = ShareSocks(item);
                        break;
                    case EConfigType.Trojan:
                        url = ShareTrojan(item);
                        break;
                    case EConfigType.VLESS:
                        url = ShareVLESS(item);
                        break;
                    default:
                        break;
                }
                return url;
            }
            catch (Exception ex)
            {
                Utils.SaveLog(ex.Message, ex);
                return "";
            }
        }

        private static string ShareVmess(ProfileItem item)
        {
            string url = string.Empty;

            VmessQRCode vmessQRCode = new VmessQRCode
            {
                v = item.configVersion.ToString(),
                ps = item.remarks.TrimEx(), //备注也许很长 ;
                add = item.address,
                port = item.port.ToString(),
                id = item.id,
                aid = item.alterId.ToString(),
                scy = item.security,
                net = item.network,
                type = item.headerType,
                host = item.requestHost,
                path = item.path,
                tls = item.streamSecurity,
                sni = item.sni,
                alpn = item.alpn,
                fp = item.fingerprint
            };

            url = Utils.ToJson(vmessQRCode);
            url = Utils.Base64Encode(url);
            url = $"{Global.vmessProtocol}{url}";

            return url;
        }

        private static string ShareShadowsocks(ProfileItem item)
        {
            string url = string.Empty;

            string remark = string.Empty;
            if (!Utils.IsNullOrEmpty(item.remarks))
            {
                remark = "#" + Utils.UrlEncode(item.remarks);
            }
            //url = string.Format("{0}:{1}@{2}:{3}",
            //    item.security,
            //    item.id,
            //    item.address,
            //    item.port);
            //url = Utils.Base64Encode(url);
            //new Sip002
            var pw = Utils.Base64Encode($"{item.security}:{item.id}");
            url = $"{pw}@{GetIpv6(item.address)}:{item.port}";
            url = $"{Global.ssProtocol}{url}{remark}";
            return url;
        }

        private static string ShareSocks(ProfileItem item)
        {
            string url = string.Empty;
            string remark = string.Empty;
            if (!Utils.IsNullOrEmpty(item.remarks))
            {
                remark = "#" + Utils.UrlEncode(item.remarks);
            }
            //url = string.Format("{0}:{1}@{2}:{3}",
            //    item.security,
            //    item.id,
            //    item.address,
            //    item.port);
            //url = Utils.Base64Encode(url);
            //new
            var pw = Utils.Base64Encode($"{item.security}:{item.id}");
            url = $"{pw}@{GetIpv6(item.address)}:{item.port}";
            url = $"{Global.socksProtocol}{url}{remark}";
            return url;
        }

        private static string ShareTrojan(ProfileItem item)
        {
            string url = string.Empty;
            string remark = string.Empty;
            if (!Utils.IsNullOrEmpty(item.remarks))
            {
                remark = "#" + Utils.UrlEncode(item.remarks);
            }
            var dicQuery = new Dictionary<string, string>();
            GetStdTransport(item, null, ref dicQuery);
            string query = "?" + string.Join("&", dicQuery.Select(x => x.Key + "=" + x.Value).ToArray());

            url = string.Format("{0}@{1}:{2}",
            item.id,
            GetIpv6(item.address),
            item.port);
            url = $"{Global.trojanProtocol}{url}{query}{remark}";
            return url;
        }

        private static string ShareVLESS(ProfileItem item)
        {
            string url = string.Empty;
            string remark = string.Empty;
            if (!Utils.IsNullOrEmpty(item.remarks))
            {
                remark = "#" + Utils.UrlEncode(item.remarks);
            }
            var dicQuery = new Dictionary<string, string>();
            if (!Utils.IsNullOrEmpty(item.security))
            {
                dicQuery.Add("encryption", item.security);
            }
            else
            {
                dicQuery.Add("encryption", "none");
            }
            GetStdTransport(item, "none", ref dicQuery);
            string query = "?" + string.Join("&", dicQuery.Select(x => x.Key + "=" + x.Value).ToArray());

            url = string.Format("{0}@{1}:{2}",
            item.id,
            GetIpv6(item.address),
            item.port);
            url = $"{Global.vlessProtocol}{url}{query}{remark}";
            return url;
        }
        private static string GetIpv6(string address)
        {
            return Utils.IsIpv6(address) ? $"[{address}]" : address;
        }

        private static int GetStdTransport(ProfileItem item, string securityDef, ref Dictionary<string, string> dicQuery)
        {
            if (!Utils.IsNullOrEmpty(item.flow))
            {
                dicQuery.Add("flow", item.flow);
            }

            if (!Utils.IsNullOrEmpty(item.streamSecurity))
            {
                dicQuery.Add("security", item.streamSecurity);
            }
            else
            {
                if (securityDef != null)
                {
                    dicQuery.Add("security", securityDef);
                }
            }
            if (!Utils.IsNullOrEmpty(item.sni))
            {
                dicQuery.Add("sni", item.sni);
            }
            if (!Utils.IsNullOrEmpty(item.alpn))
            {
                dicQuery.Add("alpn", Utils.UrlEncode(item.alpn));
            }
            if (!Utils.IsNullOrEmpty(item.fingerprint))
            {
                dicQuery.Add("fp", Utils.UrlEncode(item.fingerprint));
            }

            dicQuery.Add("type", !Utils.IsNullOrEmpty(item.network) ? item.network : "tcp");

            switch (item.network)
            {
                case "tcp":
                    dicQuery.Add("headerType", !Utils.IsNullOrEmpty(item.headerType) ? item.headerType : "none");
                    if (!Utils.IsNullOrEmpty(item.requestHost))
                    {
                        dicQuery.Add("host", Utils.UrlEncode(item.requestHost));
                    }
                    break;
                case "kcp":
                    dicQuery.Add("headerType", !Utils.IsNullOrEmpty(item.headerType) ? item.headerType : "none");
                    if (!Utils.IsNullOrEmpty(item.path))
                    {
                        dicQuery.Add("seed", Utils.UrlEncode(item.path));
                    }
                    break;

                case "ws":
                    if (!Utils.IsNullOrEmpty(item.requestHost))
                    {
                        dicQuery.Add("host", Utils.UrlEncode(item.requestHost));
                    }
                    if (!Utils.IsNullOrEmpty(item.path))
                    {
                        dicQuery.Add("path", Utils.UrlEncode(item.path));
                    }
                    break;

                case "http":
                case "h2":
                    dicQuery["type"] = "http";
                    if (!Utils.IsNullOrEmpty(item.requestHost))
                    {
                        dicQuery.Add("host", Utils.UrlEncode(item.requestHost));
                    }
                    if (!Utils.IsNullOrEmpty(item.path))
                    {
                        dicQuery.Add("path", Utils.UrlEncode(item.path));
                    }
                    break;

                case "quic":
                    dicQuery.Add("headerType", !Utils.IsNullOrEmpty(item.headerType) ? item.headerType : "none");
                    dicQuery.Add("quicSecurity", Utils.UrlEncode(item.requestHost));
                    dicQuery.Add("key", Utils.UrlEncode(item.path));
                    break;
                case "grpc":
                    if (!Utils.IsNullOrEmpty(item.path))
                    {
                        dicQuery.Add("serviceName", Utils.UrlEncode(item.path));
                        if (item.headerType == Global.GrpcgunMode || item.headerType == Global.GrpcmultiMode)
                        {
                            dicQuery.Add("mode", Utils.UrlEncode(item.headerType));
                        }
                    }
                    break;
            }
            return 0;
        }

        #endregion

        #region  ImportShareUrl 


        /// <summary>
        /// 从剪贴板导入URL
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static ProfileItem ImportFromClipboardConfig(string clipboardData, out string msg)
        {
            msg = string.Empty;
            ProfileItem profileItem = new ProfileItem();

            try
            {
                //载入配置文件 
                string result = clipboardData.TrimEx();// Utils.GetClipboardData();
                if (Utils.IsNullOrEmpty(result))
                {
                    msg = ResUI.FailedReadConfiguration;
                    return null;
                }

                if (result.StartsWith(Global.vmessProtocol))
                {
                    int indexSplit = result.IndexOf("?");
                    if (indexSplit > 0)
                    {
                        profileItem = ResolveStdVmess(result) ?? ResolveVmess4Kitsunebi(result);
                    }
                    else
                    {
                        profileItem = ResolveVmess(result, out msg);
                    }

                }
                else if (result.StartsWith(Global.ssProtocol))
                {
                    msg = ResUI.ConfigurationFormatIncorrect;

                    profileItem = ResolveSSLegacy(result) ?? ResolveSip002(result);
                    if (profileItem == null)
                    {
                        return null;
                    }
                    if (profileItem.address.Length == 0 || profileItem.port == 0 || profileItem.security.Length == 0 || profileItem.id.Length == 0)
                    {
                        return null;
                    }

                    profileItem.configType = EConfigType.Shadowsocks;
                }
                else if (result.StartsWith(Global.socksProtocol))
                {
                    msg = ResUI.ConfigurationFormatIncorrect;

                    profileItem = ResolveSocksNew(result) ?? ResolveSocks(result);
                    if (profileItem == null)
                    {
                        return null;
                    }
                    if (profileItem.address.Length == 0 || profileItem.port == 0)
                    {
                        return null;
                    }

                    profileItem.configType = EConfigType.Socks;
                }
                else if (result.StartsWith(Global.trojanProtocol))
                {
                    msg = ResUI.ConfigurationFormatIncorrect;

                    profileItem = ResolveTrojan(result);
                }
                else if (result.StartsWith(Global.vlessProtocol))
                {
                    profileItem = ResolveStdVLESS(result);

                }
                else
                {
                    msg = ResUI.NonvmessOrssProtocol;
                    return null;
                }
            }
            catch (Exception ex)
            {
                Utils.SaveLog(ex.Message, ex);
                msg = ResUI.Incorrectconfiguration;
                return null;
            }

            return profileItem;
        }

        private static ProfileItem ResolveVmess(string result, out string msg)
        {
            msg = string.Empty;
            var profileItem = new ProfileItem
            {
                configType = EConfigType.VMess
            };

            result = result.Substring(Global.vmessProtocol.Length);
            result = Utils.Base64Decode(result);

            //转成Json
            VmessQRCode vmessQRCode = Utils.FromJson<VmessQRCode>(result);
            if (vmessQRCode == null)
            {
                msg = ResUI.FailedConversionConfiguration;
                return null;
            }

            profileItem.network = Global.DefaultNetwork;
            profileItem.headerType = Global.None;

            profileItem.configVersion = Utils.ToInt(vmessQRCode.v);
            profileItem.remarks = Utils.ToString(vmessQRCode.ps);
            profileItem.address = Utils.ToString(vmessQRCode.add);
            profileItem.port = Utils.ToInt(vmessQRCode.port);
            profileItem.id = Utils.ToString(vmessQRCode.id);
            profileItem.alterId = Utils.ToInt(vmessQRCode.aid);
            profileItem.security = Utils.ToString(vmessQRCode.scy);

            profileItem.security = !Utils.IsNullOrEmpty(vmessQRCode.scy) ? vmessQRCode.scy : Global.DefaultSecurity;
            if (!Utils.IsNullOrEmpty(vmessQRCode.net))
            {
                profileItem.network = vmessQRCode.net;
            }
            if (!Utils.IsNullOrEmpty(vmessQRCode.type))
            {
                profileItem.headerType = vmessQRCode.type;
            }

            profileItem.requestHost = Utils.ToString(vmessQRCode.host);
            profileItem.path = Utils.ToString(vmessQRCode.path);
            profileItem.streamSecurity = Utils.ToString(vmessQRCode.tls);
            profileItem.sni = Utils.ToString(vmessQRCode.sni);
            profileItem.alpn = Utils.ToString(vmessQRCode.alpn);
            profileItem.fingerprint = Utils.ToString(vmessQRCode.fp);

            return profileItem;
        }

        private static ProfileItem ResolveVmess4Kitsunebi(string result)
        {
            ProfileItem profileItem = new ProfileItem
            {
                configType = EConfigType.VMess
            };
            result = result.Substring(Global.vmessProtocol.Length);
            int indexSplit = result.IndexOf("?");
            if (indexSplit > 0)
            {
                result = result.Substring(0, indexSplit);
            }
            result = Utils.Base64Decode(result);

            string[] arr1 = result.Split('@');
            if (arr1.Length != 2)
            {
                return null;
            }
            string[] arr21 = arr1[0].Split(':');
            string[] arr22 = arr1[1].Split(':');
            if (arr21.Length != 2 || arr21.Length != 2)
            {
                return null;
            }

            profileItem.address = arr22[0];
            profileItem.port = Utils.ToInt(arr22[1]);
            profileItem.security = arr21[0];
            profileItem.id = arr21[1];

            profileItem.network = Global.DefaultNetwork;
            profileItem.headerType = Global.None;
            profileItem.remarks = "Alien";

            return profileItem;
        }

        private static ProfileItem ResolveStdVmess(string result)
        {
            ProfileItem i = new ProfileItem
            {
                configType = EConfigType.VMess,
                security = "auto"
            };

            Uri u = new Uri(result);

            i.address = u.IdnHost;
            i.port = u.Port;
            i.remarks = u.GetComponents(UriComponents.Fragment, UriFormat.Unescaped);
            var q = HttpUtility.ParseQueryString(u.Query);

            var m = StdVmessUserInfo.Match(u.UserInfo);
            if (!m.Success) return null;

            i.id = m.Groups["id"].Value;

            if (m.Groups["streamSecurity"].Success)
            {
                i.streamSecurity = m.Groups["streamSecurity"].Value;
            }
            switch (i.streamSecurity)
            {
                case "tls":
                    // TODO tls config
                    break;
                default:
                    if (!string.IsNullOrWhiteSpace(i.streamSecurity))
                        return null;
                    break;
            }

            i.network = m.Groups["network"].Value;
            switch (i.network)
            {
                case "tcp":
                    string t1 = q["type"] ?? "none";
                    i.headerType = t1;
                    // TODO http option

                    break;
                case "kcp":
                    i.headerType = q["type"] ?? "none";
                    // TODO kcp seed
                    break;

                case "ws":
                    string p1 = q["path"] ?? "/";
                    string h1 = q["host"] ?? "";
                    i.requestHost = Utils.UrlDecode(h1);
                    i.path = p1;
                    break;

                case "http":
                case "h2":
                    i.network = "h2";
                    string p2 = q["path"] ?? "/";
                    string h2 = q["host"] ?? "";
                    i.requestHost = Utils.UrlDecode(h2);
                    i.path = p2;
                    break;

                case "quic":
                    string s = q["security"] ?? "none";
                    string k = q["key"] ?? "";
                    string t3 = q["type"] ?? "none";
                    i.headerType = t3;
                    i.requestHost = Utils.UrlDecode(s);
                    i.path = k;
                    break;

                default:
                    return null;
            }

            return i;
        }

        private static ProfileItem ResolveSip002(string result)
        {
            Uri parsedUrl;
            try
            {
                parsedUrl = new Uri(result);
            }
            catch (UriFormatException)
            {
                return null;
            }
            ProfileItem server = new ProfileItem
            {
                remarks = parsedUrl.GetComponents(UriComponents.Fragment, UriFormat.Unescaped),
                address = parsedUrl.IdnHost,
                port = parsedUrl.Port,
            };
            string rawUserInfo = parsedUrl.GetComponents(UriComponents.UserInfo, UriFormat.UriEscaped);
            //2022-blake3
            if (rawUserInfo.Contains(":"))
            {
                string[] userInfoParts = rawUserInfo.Split(new[] { ':' }, 2);
                if (userInfoParts.Length != 2)
                {
                    return null;
                }
                server.security = userInfoParts[0];
                server.id = Utils.UrlDecode(userInfoParts[1]);
            }
            else
            {
                // parse base64 UserInfo
                string userInfo = Utils.Base64Decode(rawUserInfo);
                string[] userInfoParts = userInfo.Split(new[] { ':' }, 2);
                if (userInfoParts.Length != 2)
                {
                    return null;
                }
                server.security = userInfoParts[0];
                server.id = userInfoParts[1];
            }

            NameValueCollection queryParameters = HttpUtility.ParseQueryString(parsedUrl.Query);
            if (queryParameters["plugin"] != null)
            {
                //obfs-host exists
                var obfsHost = queryParameters["plugin"].Split(';').FirstOrDefault(t => t.Contains("obfs-host"));
                if (queryParameters["plugin"].Contains("obfs=http") && !Utils.IsNullOrEmpty(obfsHost))
                {
                    obfsHost = obfsHost.Replace("obfs-host=", "");
                    server.network = Global.DefaultNetwork;
                    server.headerType = Global.TcpHeaderHttp;
                    server.requestHost = obfsHost;
                }
                else
                {
                    return null;
                }
            }

            return server;
        }

        private static readonly Regex UrlFinder = new Regex(@"ss://(?<base64>[A-Za-z0-9+-/=_]+)(?:#(?<tag>\S+))?", RegexOptions.IgnoreCase);
        private static readonly Regex DetailsParser = new Regex(@"^((?<method>.+?):(?<password>.*)@(?<hostname>.+?):(?<port>\d+?))$", RegexOptions.IgnoreCase);

        private static ProfileItem ResolveSSLegacy(string result)
        {
            var match = UrlFinder.Match(result);
            if (!match.Success)
                return null;

            ProfileItem server = new ProfileItem();
            var base64 = match.Groups["base64"].Value.TrimEnd('/');
            var tag = match.Groups["tag"].Value;
            if (!Utils.IsNullOrEmpty(tag))
            {
                server.remarks = Utils.UrlDecode(tag);
            }
            Match details;
            try
            {
                details = DetailsParser.Match(Utils.Base64Decode(base64));
            }
            catch (FormatException)
            {
                return null;
            }
            if (!details.Success)
                return null;
            server.security = details.Groups["method"].Value;
            server.id = details.Groups["password"].Value;
            server.address = details.Groups["hostname"].Value;
            server.port = int.Parse(details.Groups["port"].Value);
            return server;
        }


        private static readonly Regex StdVmessUserInfo = new Regex(
            @"^(?<network>[a-z]+)(\+(?<streamSecurity>[a-z]+))?:(?<id>[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})$");

        private static ProfileItem ResolveSocks(string result)
        {
            ProfileItem profileItem = new ProfileItem
            {
                configType = EConfigType.Socks
            };
            result = result.Substring(Global.socksProtocol.Length);
            //remark
            int indexRemark = result.IndexOf("#");
            if (indexRemark > 0)
            {
                try
                {
                    profileItem.remarks = Utils.UrlDecode(result.Substring(indexRemark + 1, result.Length - indexRemark - 1));
                }
                catch { }
                result = result.Substring(0, indexRemark);
            }
            //part decode
            int indexS = result.IndexOf("@");
            if (indexS > 0)
            {
            }
            else
            {
                result = Utils.Base64Decode(result);
            }

            string[] arr1 = result.Split('@');
            if (arr1.Length != 2)
            {
                return null;
            }
            string[] arr21 = arr1[0].Split(':');
            //string[] arr22 = arr1[1].Split(':');
            int indexPort = arr1[1].LastIndexOf(":");
            if (arr21.Length != 2 || indexPort < 0)
            {
                return null;
            }
            profileItem.address = arr1[1].Substring(0, indexPort);
            profileItem.port = Utils.ToInt(arr1[1].Substring(indexPort + 1, arr1[1].Length - (indexPort + 1)));
            profileItem.security = arr21[0];
            profileItem.id = arr21[1];

            return profileItem;
        }

        private static ProfileItem ResolveSocksNew(string result)
        {
            Uri parsedUrl;
            try
            {
                parsedUrl = new Uri(result);
            }
            catch (UriFormatException)
            {
                return null;
            }
            ProfileItem server = new ProfileItem
            {
                remarks = parsedUrl.GetComponents(UriComponents.Fragment, UriFormat.Unescaped),
                address = parsedUrl.IdnHost,
                port = parsedUrl.Port,
            };

            // parse base64 UserInfo
            string rawUserInfo = parsedUrl.GetComponents(UriComponents.UserInfo, UriFormat.Unescaped);
            string userInfo = Utils.Base64Decode(rawUserInfo);
            string[] userInfoParts = userInfo.Split(new[] { ':' }, 2);
            if (userInfoParts.Length == 2)
            {
                server.security = userInfoParts[0];
                server.id = userInfoParts[1];
            }

            return server;
        }

        private static ProfileItem ResolveTrojan(string result)
        {
            ProfileItem item = new ProfileItem
            {
                configType = EConfigType.Trojan
            };

            Uri url = new Uri(result);

            item.address = url.IdnHost;
            item.port = url.Port;
            item.remarks = url.GetComponents(UriComponents.Fragment, UriFormat.Unescaped);
            item.id = url.UserInfo;

            var query = HttpUtility.ParseQueryString(url.Query);
            ResolveStdTransport(query, ref item);

            return item;
        }
        private static ProfileItem ResolveStdVLESS(string result)
        {
            ProfileItem item = new ProfileItem
            {
                configType = EConfigType.VLESS,
                security = "none"
            };

            Uri url = new Uri(result);

            item.address = url.IdnHost;
            item.port = url.Port;
            item.remarks = url.GetComponents(UriComponents.Fragment, UriFormat.Unescaped);
            item.id = url.UserInfo;

            var query = HttpUtility.ParseQueryString(url.Query);
            item.security = query["encryption"] ?? "none";
            item.streamSecurity = query["security"] ?? "";
            ResolveStdTransport(query, ref item);

            return item;
        }

        private static int ResolveStdTransport(NameValueCollection query, ref ProfileItem item)
        {
            item.flow = query["flow"] ?? "";
            item.streamSecurity = query["security"] ?? "";
            item.sni = query["sni"] ?? "";
            item.alpn = Utils.UrlDecode(query["alpn"] ?? "");
            item.fingerprint = Utils.UrlDecode(query["fp"] ?? "");
            item.network = query["type"] ?? "tcp";
            switch (item.network)
            {
                case "tcp":
                    item.headerType = query["headerType"] ?? "none";
                    item.requestHost = Utils.UrlDecode(query["host"] ?? "");

                    break;
                case "kcp":
                    item.headerType = query["headerType"] ?? "none";
                    item.path = Utils.UrlDecode(query["seed"] ?? "");
                    break;

                case "ws":
                    item.requestHost = Utils.UrlDecode(query["host"] ?? "");
                    item.path = Utils.UrlDecode(query["path"] ?? "/");
                    break;

                case "http":
                case "h2":
                    item.network = "h2";
                    item.requestHost = Utils.UrlDecode(query["host"] ?? "");
                    item.path = Utils.UrlDecode(query["path"] ?? "/");
                    break;

                case "quic":
                    item.headerType = query["headerType"] ?? "none";
                    item.requestHost = query["quicSecurity"] ?? "none";
                    item.path = Utils.UrlDecode(query["key"] ?? "");
                    break;
                case "grpc":
                    item.path = Utils.UrlDecode(query["serviceName"] ?? "");
                    item.headerType = Utils.UrlDecode(query["mode"] ?? Global.GrpcgunMode);
                    break;
                default:
                    break;
            }
            return 0;
        }

        #endregion
    }
}
