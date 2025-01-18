FROM alpine:latest AS build

# The docker context must be the folder with published files so we can zip them
# The folder must also contain sub-folder Ccs3ClientApp containing the published client app files
COPY Ccs3ClientAppWindowsService.exe /usr/ccs3-published-files/
COPY appsettings.json /usr/ccs3-published-files/
COPY Ccs3ClientApp/*.* /usr/ccs3-published-files/Ccs3ClientApp/
RUN apk update && apk --no-cache add zip
RUN mkdir /usr/ccs3-zipped-files
RUN cd /usr/ccs3-published-files/ && zip -r -9 /usr/ccs3-zipped-files/Ccs3ClientAppWindowsService.zip .
RUN sha256sum /usr/ccs3-zipped-files/Ccs3ClientAppWindowsService.zip > /usr/ccs3-zipped-files/Ccs3ClientAppWindowsService.sha

FROM alpine:latest
COPY --from=build /usr/ccs3-zipped-files /usr/ccs3-zipped-files