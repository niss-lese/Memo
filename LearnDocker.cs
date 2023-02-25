# region Chapter 1
** Clean up
- docker container rm -f $(docker container ls -aq)
- docker image rm -f $(docker image ls -f reference='diamol/*' -q)
#endregion

# region Chapter 2
** Starting an interactive with terminal connection
docker container run --interactive --tty diamol/base
docker container run -ti diamol/base // (same as above interactive terminal)

** Shows processes running in the container
docker container top sha...

** Shows log entries
docker container logs sha...

** Shows details of a container
docker container inspect sha...

** Shows all containers
docker container ls --all

** Shows stats about a container
docker container stats sha...

docker container run --detach --publish 8088:80 diamol/ch02-hellodiamol-web
 --detach—Starts the container in the background and shows the container ID
 --publish—Publishes a port from the container to the computer

** LAB :: Replace the index file of the server with something else
docker container cp index.html 86b:/usr/local/apache2/htdocs/index.html

The format of the cp command is [source path] [target path].
The container can be the source or the target, and you prefix 
the container file path with the container ID (86b here).
You can use the same file path format with forward-slashes on Linux or Windows in the cp command.
::
If you're using Windows containers on Windows 10 you may get the error filesystem operations
against a running Hyper-V container are not supported, which means you'll need to stop the container
with docker container stop <id> before you run the docker container cp command,
and then start the container again afterwards with docker container start <id>.
#endregion

# region Chapter 3
** --name flag enable the naming of container
docker container run -d --name web-ping diamol/ch03-web-ping

** --env flag for defining environment variable, here TARGET ist being set to google.com
*** (-e short for --env)
docker container run -d --name web-ping --env TARGET=google.com diamol/ch03-web-ping

** FIRST DOCKERFILE
FROM diamol/node
ENV TARGET="blog.sixeyed.com"
ENV METHOD="HEAD"
ENV INTERVAL="3000"
WORKDIR /web-ping
COPY app.js .
CMD ["node", "/web-ping/app.js"]

	FROM—Every image has to start from another image. In this case, the web-ping
	image will use the diamol/node image as its starting point. That image has
	Node.js installed, which is everything the web-ping application needs to run.
	
	ENV—Sets values for environment variables. The syntax is [key]="[value]",
	and there are three ENV instructions here, setting up three different environment variables.
	
	WORKDIR—Creates a directory in the container image filesystem, and sets that to
	be the current working directory. The forward-slash syntax works for Linux and
	Windows containers, so this will create /web-ping on Linux and C:\web-ping
	on Windows.
	
	COPY—Copies files or directories from the local filesystem into the container	
	image. The syntax is [source path] [target path]—in this case, I’m copying
	app.js from my local machine into the working directory in the image.
	
	CMD—Specifies the command to run when Docker starts a container from the
	image. This runs Node.js, starting the application code in app.js.

** Building docker image. --tag specify the name of the image
*** Using dockerfile from above and assuming resources are all available
docker image build --tag web-pinging .

** Check image build history
docker image history web-pinging

** Shows how much disk space docker is actually using
docker system df

** OPTIMIZE DOCKERFILE
Put command that changes at the bottom and those that do not at the top.
The CMD instruction doesn’t need to be at the end of the Dockerfile; it can
be anywhere after the FROM instruction and still have the same result. It’s unlikely to
change, so you can move it nearer the top. And one ENV instruction can be used to set
multiple environment variables, so the three separate ENV instructions can be combined.

FROM diamol/node
CMD ["node", "/web-ping/app.js"]
ENV TARGET="blog.sixeyed.com" \
METHOD="HEAD" \
INTERVAL="3000"
WORKDIR /web-ping
COPY app.js .

LAB ::
Container can be used to produce new images.
* Start a container, make some changes then exit.
- docker container commit containerName newContainerName
- docker container run newContainerName

#endregion