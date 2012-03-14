::===============
::Writes
::===============
sqlio -kW -t2 -s30 -o1 -frandom -b8 -BH -LS -Fparam.txt
sqlio -kW -t2 -s30 -o2 -frandom -b8 -BH -LS -Fparam.txt
sqlio -kW -t2 -s30 -o4 -frandom -b8 -BH -LS -Fparam.txt
sqlio -kW -t2 -s30 -o8 -frandom -b8 -BH -LS -Fparam.txt
sqlio -kW -t2 -s30 -o16 -frandom -b8 -BH -LS -Fparam.txt

sqlio -kW -t2 -s30 -o1 -fsequential -b8 -BH -LS -Fparam.txt
sqlio -kW -t2 -s30 -o2 -fsequential -b8 -BH -LS -Fparam.txt
sqlio -kW -t2 -s30 -o4 -fsequential -b8 -BH -LS -Fparam.txt
sqlio -kW -t2 -s30 -o8 -fsequential -b8 -BH -LS -Fparam.txt
sqlio -kW -t2 -s30 -o16 -fsequential -b8 -BH -LS -Fparam.txt

::===============
::Reads
::===============
sqlio -kR -t2 -s30 -o1 -frandom -b8 -BH -LS -Fparam.txt
sqlio -kR -t2 -s30 -o2 -frandom -b8 -BH -LS -Fparam.txt
sqlio -kR -t2 -s30 -o4 -frandom -b8 -BH -LS -Fparam.txt
sqlio -kR -t2 -s30 -o8 -frandom -b8 -BH -LS -Fparam.txt
sqlio -kR -t2 -s30 -o16 -frandom -b8 -BH -LS -Fparam.txt

sqlio -kR -t2 -s30 -o1 -fsequential -b8 -BH -LS -Fparam.txt
sqlio -kR -t2 -s30 -o2 -fsequential -b8 -BH -LS -Fparam.txt
sqlio -kR -t2 -s30 -o4 -fsequential -b8 -BH -LS -Fparam.txt
sqlio -kR -t2 -s30 -o8 -fsequential -b8 -BH -LS -Fparam.txt
sqlio -kR -t2 -s30 -o16 -fsequential -b8 -BH -LS -Fparam.txt