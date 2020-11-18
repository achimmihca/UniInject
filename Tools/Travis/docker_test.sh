#!/usr/bin/env bash

set -e

docker run \
  -e UNITY_LICENSE_CONTENT \
  -e TEST_PLATFORM \
  -w /project/ \
  -v $(pwd):/project/ \
  $IMAGE_NAME \
  /bin/bash -c "/project/Tools/Travis/before_script.sh && /project/Tools/Travis/test.sh"