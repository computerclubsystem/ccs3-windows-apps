FROM alpine:latest AS build

# The docker context must be the folder with published files so we can zip them
COPY Ccs3ClientAppWindowsService.exe /usr/ccs3-published-files/
COPY appsettings.json /usr/ccs3-published-files/
RUN apk update && apk --no-cache add zip
RUN mkdir /usr/ccs3-zipped-files
RUN zip -j -9 /usr/ccs3-zipped-files/Ccs3ClientAppWindowsService.zip /usr/ccs3-published-files/*.*
RUN sha256sum /usr/ccs3-zipped-files/Ccs3ClientAppWindowsService.zip > /usr/ccs3-zipped-files/Ccs3ClientAppWindowsService.sha

FROM alpine:latest
COPY --from=build /usr/ccs3-zipped-files /usr/ccs3-zipped-files