#!/bin/sh



dir=`mktemp -d`

#wget --no-check-certificate -O "$dir/swagger.json" --no-verbose  https://localhost:5001/swagger/v1/swagger.json || exit -1
wget --no-check-certificate -O "$dir/swagger.json" --no-verbose  http://localhost:5000/swagger/v1/swagger.json || exit -1


docker run --rm --user $(id -u):$(id -g) -v ${dir}:/local swaggerapi/swagger-codegen-cli-v3 generate -i /local/swagger.json -l typescript-angular -o /local/swagger/ --additional-properties ngVersion=10


rm -rf swagger.old
mv swagger swagger.old

cp -a "$dir/swagger" ./
# make fixes to generated code
sed -i 's/ModuleWithProviders {/ModuleWithProviders<ApiModule> {/' "./swagger/api.module.ts"
echo 'export interface ProblemDetails {}' >"./swagger/model/problemDetails.ts"

rm -rf "$dir"

