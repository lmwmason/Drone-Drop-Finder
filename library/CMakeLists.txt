cmake_minimum_required(VERSION 3.16)
project(drone_drop C)

set(CMAKE_C_STANDARD 99)

add_library(drone_drop STATIC
    drone_drop.c
)

target_include_directories(drone_drop PUBLIC ${CMAKE_CURRENT_SOURCE_DIR})
target_link_libraries(drone_drop m)

add_executable(drone_drop_example example.c)
target_link_libraries(drone_drop_example drone_drop)
