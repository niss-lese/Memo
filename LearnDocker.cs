# region Chapter_1
** Common
- docker info
- docker container rm -f $(docker container ls -aq)
- docker image rm -f $(docker image ls -f reference='diamol/*' -q)
- docker image ls --filter reference=my-image-name --filter reference='*/another-image-name'
#endregion


# region Chapter_2
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


# region Chapter_3
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


# region Chapter_4
* Multi-Stage Dockerfile

FROM diamol/base AS build-stage
RUN echo 'Building...' > /build.txt
FROM diamol/base AS test-stage
COPY --from=build-stage /build.txt /build.txt
RUN echo 'Testing...' >> /build.txt
FROM diamol/base
COPY --from=test-stage /build.txt /build.txt
CMD cat /build.txt


* Docker Network
docker network create
* Running a container and connecting it to the network 
docker container run --name iotd -d -p 800:80 --network nat myImage


* OPTIMIZING DOCKERFILE
** File to optimize
FROM diamol/golang 

WORKDIR web
COPY index.html .
COPY main.go .

RUN go build -o /web/server
RUN chmod +x /web/server

CMD ["/web/server"]
ENV USER=sixeyed
EXPOSE 80

** Optimized file
FROM diamol/golang AS builder

COPY main.go .
RUN go build -o /server
RUN chmod +x /server

# app
FROM diamol/base

EXPOSE 80
CMD ["/web/server"]
ENV USER="sixeyed"

WORKDIR web
COPY --from=builder /server .
COPY index.html .

#endregion


# region Chapter_5
* Login into dockerhub
** $dockerId is the dockerhub username
docker login --username $dockerId

* Creating a new reference for existing image, along with tag version
docker image tag my-old-image $dockerId/my-new-image:tag-version

* Pushing image to dockerhub
docker image push $dockerId/image-gallery:v01

* Running Docker registry locally
** Using diamol/registry
docker container run -d -p 5000:5000 --restart always diamol/registry

* Giving computer a name, instead of local host
# using PowerShell on Windows
Add-Content -Value "127.0.0.1 registry.local" -Path /windows/system32/drivers/etc/hosts
# using Bash on Linux or Mac
echo $'\n127.0.0.1 registry.local' | sudo tee -a /etc/hosts

	If you get a permissions error from that command, you’ll need to be logged in with
	administrator privileges in an elevated PowerShell session on Windows, or use sudo on
	Linux or Mac.
	
* Creating new image for local registry
** Local registry doesn’t have any authentication or authorization set up
docker image tag image-gallery registry.local:5000/gallery/ui:v1
**
	Three containers make up the NASA
	image-of-the-day app in chapter 4—you could tag all the images to group them
	together using gallery as the project name:
 registry.local:5000/gallery/ui:v1—The Go web UI
 registry.local:5000/gallery/api:v1—The Java API
 registry.local:5000/gallery/logs:v1—The Node.js API

	Docker won’t communicate with an unencrypted registry by
	default, because it’s not secure. You need to explicitly add your registry domain to a
	list of permitted insecure registries before Docker will let you use it.
	This brings us to configuring Docker. The Docker Engine uses a JSON configuration file for
	all sorts of settings, including where Docker stores the image layers on disk, where the Docker
	API listens for connections, and which insecure
	registries are permitted. The file is called daemon.json and it usually lives in the
	folder C:\ProgramData\docker\config on Windows Server, and /etc/docker on
	Linux. You can edit that file directly, but if you’re using Docker Desktop on Mac
	or Windows, you’ll need use the UI, where you can change the main configuration settings.
	TRY IT NOW Right-click the Docker whale icon in your taskbar, and select Settings (or Preferences on the Mac).
	Then open the Daemon tab and enter
	registry.local:5000 in the insecure registries list
	
	Then restart Docker using Restart-Service docker on Windows Server, or service
	docker restart on Linux

* Pushing image to local registry
docker image push registry.local:5000/gallery/ui:v1

* Versioning of tags
	The basic idea is something like [major].[minor].[patch], which has
	some implicit guarantees. A release that only increments the patch number might
	have bug fixes, but it should have the same features as the last version; a release that
	increments the minor version might add features but shouldn’t remove any; and a
	major release could have completely different features.
	
* LAB
** deleteimage from local registry
- List images on local registry
curl http://registry.local:5000/v2/gallery/ui/tags/list
- Delete did not work 
*** https://github.com/sixeyed/diamol/tree/master/ch05/lab
#endregion


