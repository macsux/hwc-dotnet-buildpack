Hosted Web Core (HWC) buildpack that can run Windows IIS based applications on Windows stack.  

### Why would I want to use this over  [official hwc buildpack](https://github.com/cloudfoundry/hwc-buildpack)?

1. You need to run 32 bit app. Releases page publishes both x64 and x86 versions of the HWC
2. All config sections of IIS are unlocked, allowing them to be overridden in web.config inside the app
3. You want to modify it and not comfortable doing it in Go

### What is HWC?

HWC is an API that allows running IIS Core in a process that is not IIS worker.  [See more info](https://docs.microsoft.com/en-us/iis/web-development-reference/native-code-api-reference/hosted-web-core-api-reference)