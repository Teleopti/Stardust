<%@ Page Language="C#" %>
<% Response.StatusCode = 404; %>
<!doctype html>
<html lang="en">

<head>
	<meta charset="utf-8" />
	<meta http-equiv="X-UA-Compatible" content="IE=edge,chrome=1" />
	<meta name="viewport" content="width=device-width, initial-scale=1">
	<meta charset="utf-8">
	<title>Page not found</title>
</head>
<style>

	html, body{
	height: 100%;
	overflow-x: hidden;
}
	body {
		background: #303030;
		/*background-image: url();
		background-position: center center;
		background-repeat: no-repeat;
		background-attachment: fixed;
		background-size: cover;*/
		margin: 0;
		padding: 0;
	}

.center{
	text-align: center!important;
}

.content {
	box-sizing: border-box;
	-moz-box-sizing: border-box;
	-webkit-box-sizing: border-box;
	width: 100%;
	top: 15%;
	position: absolute;
	color: #fff;
	font-family: Calibri,Arial,sans-serif!important;
}

h1{
	font-size: 10em;
	margin: 10px 0;
	text-shadow: 0 3px 6px rgba(0,0,0,0.16), 0 3px 6px rgba(0,0,0,0.23);
}

.btn{
	background-color: #09f;
	border:none;
	transition: 1s all ease;
	padding: 1rem;
	padding: 10px;
	margin: 5px;
	border: none;
	color: #fff;
	cursor: pointer;
	border-radius: 3px;
	box-shadow: 0 3px 6px rgba(0,0,0,0.16), 0 3px 6px rgba(0,0,0,0.23);
}

</style>

<body>

	<div class="content center">
		<h1>404</h1>
		<p>Oops! That page does not exist.</p>
		<input action="action" onclick="window.history.go(-1); return false;" type="button" class="btn" value="Back" />
	</div>

</body>

</html>