// Chris Chen (DEM) crscrs@live.com
#include "windows.h"

#include "optix_world.h"

#include "IL/il.h"
#include "IL/ilu.h"

#define TINYEXR_IMPLEMENTATION
#include "tinyexr.h"

#pragma once
#define OAD_API __declspec(dllexport) 

//typedef void(*progressCallBack)(float);
//typedef int(*completeCallBack)(BYTE*, int, int, int);

BYTE sdata[4096 * 4096 * 4];
float exrdata[4096 * 4096 * 4];
float exrimages[4][4096 * 4096 * 4];


ILuint inputImage;
ILuint outputImage;
bool running = false;

EXRHeader exrheader;

EXRImage exrimage;

optix::Context optix_context;
optix::Buffer input_buffer;
optix::Buffer output_buffer;
optix::PostprocessingStage denoiserStage;
optix::CommandList commandList;

EXTERN_C OAD_API void _stdcall _jobStart(int, int, float);
EXTERN_C OAD_API void _stdcall _jobComplete();
EXTERN_C OAD_API int _stdcall _getWidth(char[]);
EXTERN_C OAD_API int _stdcall _getHeight(char[]);
EXTERN_C OAD_API void _stdcall _setUpContext(void);
EXTERN_C OAD_API void _stdcall _cleantUpContext(void);
EXTERN_C OAD_API BYTE* _stdcall _denoiseImplement(char[], char[], float, bool);