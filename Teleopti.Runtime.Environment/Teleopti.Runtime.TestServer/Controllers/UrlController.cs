using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;

namespace Teleopti.Runtime.TestServer.Controllers
{
    public class UrlController : Controller
    {
        public ActionResult Index()
        {
            var myRSA = new RSACryptoServiceProvider();
            myRSA.FromXmlString(
                @"<RSAKeyValue><Modulus>tcQWMgdpQeCd8+gzB3rYQAehHXF5mBGdyFMkJMEmcQmTlkpg22xLNz/kNYXZ7j2Cuhls+PBORzZkfBsNoL1vErT+N9Es4EEWOt6ntNe7wujqQqktUT/QOWEMJ8zJQM3bn7Oj9H5StBr7DWSRzgEjOc7knDcb4KCQL3ceXqmqwSonPfP1hp+bE8rZuxDISYiZVEkm417YzUHBk3ppV30Q9zvfL9IZX0q/ebCTRnLFockl7yOVucomvo8j4ssFPCAYgASoNvzWq+s5UTzYELl1I7F3hQnFwx0bIpQFmGbZ5BbNczc6rVYtCX5KDMsVaJSUcXBAnqGd20hq/ICkBR658w==</Modulus><Exponent>AQAB</Exponent><P>8r2FFhgc78WZf/uKEjHPyiLL9FkcjbPsdLB/Dd6AEOVuzpVFlBJsai31gyLIUU3zY6gE/NdMZzQ7ejsjhbpC4/ptbJguTpIOGB+7dX+/DEdwZkx8rIlNG32VDIdP6kqpwPzhtGVfNiq8xHaS+SHTQf6JSWQtNKgVbilWgYyEZ9k=</P><Q>v7Hm0iP49ReGhVvKdAsDUgcK0olmVGAKwsxsFnXUGkAWydh+T3QaChYkYBS+h5cX4UBlil5FaJKtGq4wduKDkMCN8TNHh3n2k05rh4DxxPmLvhCqkQgvMB/22E+z10VAmjKPq7BnAq/lc2rGJWa1lq3qaeSkcF6agCPQVYd6vKs=</Q><DP>7mr7dvIEKfVZiX0U5j4Kq611yfBkvUHFs+9PO94Yx3+yUDIJfyCBX+D4Te8x9bmsn2t+SqFlJ9EDwlCn2UdTP/zO0WS/xuhp84PnaccpbPQWEER8CDNrit7UMNQOyD7BcQ5w2fDfjaJ4ejdEsHJqv100luNQC3I0alkr4F6WBjE=</DP><DQ>nZ2rSlGlm/BR/Ujx9+QuQL3lmiK7btjhQDZREU6krUjQ8/n8MVwnJO/7zLyBxH7pdZ47X0AQFeG0T2G2G6o3v0dz7kTZpX0Uzx4FsA7Hu8vrqMWPWVy/X/SIRGeUWYZpjd/Q3bxXlpAGO5Ypggsnd9NcEOGci4BdzMqlvA1/T60=</DQ><InverseQ>4OVuc98gImxH9yLmvMimnr9zFxw6pwRd++/A7q1Uj8BrCKhzD8rY6QosNZCKlMjWdHNYtAWrDDqxwCRPUYhmdOm+JEkxBL7yelodmongNXUS0J9kf9c+k8fjgEsB02J15QqryyuRvw0Z638BheD9Ry3NN/REO0pdnj2LHxa17V0=</InverseQ><D>YQOGoS8pc9rSE1OUoOJlN0+bI57kOlD0uO3/NYrN3Lkyx51tMtALGTMFt7d4SNsVwgQ+EGQaM5IJcd/ylx9kgESQBvSjEhJLLiKWukQG2BH+rpOjN2Fq3qU4mqmHpQn6tbNox98Af1aDNnO+Coi652jQxbv4Kh0ot9zJHddK5wuTxIQDLAxyb50f/ReG3UekxZoEOZEtj1oEd+QB/py+hP7Xp010Wfzy82g5Ec3ELjzeNPtijxmO+WExoF5zALIUYd8ClH+ayr2Ab3rD0Dv8VM1Y08npzSs6d5OOAAnG+245koiwkgJoXvZg0EVkcdJxZsMxIGL/OWEl82VnqC6mUQ==</D></RSAKeyValue>");

            var url = ConfigurationManager.AppSettings["url"];
            var result = myRSA.SignData(
                new MemoryStream(Encoding.UTF8.GetBytes(url)), CryptoConfig.MapNameToOID("SHA1"));

            return Json(new { Url = url, Signature = Convert.ToBase64String(result) }, JsonRequestBehavior.AllowGet);
        }
    }
}
