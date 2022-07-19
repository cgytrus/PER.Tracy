cmake_minimum_required(VERSION 3.13.0)
set(CMAKE_CXX_STANDARD 11)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

set(PROJECT_NAME PER.Tracy.Native)
project(${PROJECT_NAME})

file(GLOB_RECURSE SOURCES src/*.cpp)
add_library(${PROJECT_NAME} SHARED ${SOURCES})

option(TRACY_ENABLE "" ON)
option(TRACY_ON_DEMAND "" OFF)

option(TRACY_ONLY_LOCALHOST "" OFF)

option(TRACY_NO_CALLSTACK "" ON)
option(TRACY_CALLSTACK "" OFF)

add_subdirectory(lib/tracy)
target_link_libraries(${PROJECT_NAME} Tracy::TracyClient)
