import { useEffect, useState } from "react";
import { Controller, useForm } from "react-hook-form";
import { IMaskInput } from "react-imask";
import { useLocation, useNavigate, useNavigation, useSearchParams } from "react-router-dom";
import ReactSelect from "react-select";
import _mock from '../../../../assets/mock.png'
import { makePrivateRequest } from "../../../../core/script";
import { ContractType, Insurer, Parameters } from "../../../../core/types";

const retroactivities = [
    {
        value: "60", label: "Até 60 dias"
    },
    {
        value: "90", label: "Até 90 dias"
    },
    {
        value: "120", label: "Até 120 dias"
    },
    {
        value: "150", label: "Até 150 dias"
    },
    {
        value: "180", label: "Até 180 dias"
    },
    {
        value: "200", label: "Mais de 180 dias"
    }
]

const CompanyForm = () => {
    const { register, control, formState, handleSubmit, setValue } = useForm<Parameters>()
    const { pathname } = useLocation();
    const nav = useNavigate();
    const [insurer, setInsurer] = useState<Insurer>()

    const getInsurerId = () => {
        let values = pathname.substring(1).split("/")
        return Number(values[1])
    }
    const getContractType = () => {
        let values = pathname.substring(1).split("/")
        let type = values[2] === 'public' ? "PUBLIC_CONTRACT" : "PRIVATE_CONTRACT";
        return type as ContractType;
    }

    const onSubmit = (data: Parameters) => {

        const payload: Parameters = {
            ...data,
            proposalType: getContractType(),
            ccg: Number(String(data.ccg).replaceAll(".", "").replaceAll(",", ".")),
            minimumBounty: Number(String(data.minimumBounty).replaceAll(".", "").replaceAll(",", ".")),
            minimumBrokerage: Number(String(data.minimumBrokerage).replaceAll(".", "").replaceAll(",", ".")),
            maximumCommission: Number(String(data.maximumCommission).replaceAll(".", "").replaceAll(",", ".")) / 100,
            baseCommission: Number(String(data.baseCommission).replaceAll(".", "").replaceAll(",", ".")) / 100,
            externalRetroactivity: Number(String(data.externalRetroactivity).replaceAll(".", "").replaceAll(",", ".")),
        }

        console.log("DATA: ", payload)

        makePrivateRequest({ url: `/insurers/${getInsurerId()}`, data: payload, method: 'PUT' })
            .then(() => nav('/companies'))
    }

    useEffect(() => {
        makePrivateRequest({ url: `/insurers/${getInsurerId()}`, params: { type: getContractType() } })
            .then(r => {
                setInsurer(r.data)

                const parameters: Parameters = r.data.parameters![0]

                setValue('ccg', parameters.ccg)
                setValue('baseCommission', parameters.baseCommission * 100)
                setValue('maximumCommission', parameters.maximumCommission * 100)
                setValue('minimumBounty', parameters.minimumBounty)
                setValue('minimumBrokerage', parameters.minimumBrokerage)
                setValue('externalRetroactivity', parameters.externalRetroactivity)
                setValue('internalRetroactivity', parameters.internalRetroactivity)
                setValue('exclusive', parameters.exclusive)
                setValue('pstp', parameters.pstp)
                setValue('grievanceRule', parameters.grievanceRule)
            })


    }, [])

    return <form onSubmit={handleSubmit(x => onSubmit(x))} className="bg-white mx-10 my-5 p-6 border border-gray-300 rounded-xl drop-shadow-md flex flex-col">
        <div className="grid grid-cols-5 gap-1 grid-flow-row text-base">
            <div className="row-span-2 flex justify-center items-center w-auto shadow-md rounded-lg mr-2 overflow-hidden">
                {
                    insurer && insurer.picture && <img src={insurer.picture}></img>
                }
            </div>
            <label className="company-input col-span-2">
                <span>CCG (R$)</span>
                <Controller
                    name={"ccg"}
                    control={control}
                    render={({ field }) => (
                        <IMaskInput
                            mask={Number}
                            value={String(field.value)}
                            scale={2}
                            thousandsSeparator="."
                            padFractionalZeros={false}
                            normalizeZeros={true}
                            radix={","}
                            mapToRadix={["."]}
                            onComplete={field.onChange}
                        />
                    )}
                />
            </label>
            <label className="company-input col-span-2">
                <span>Prêmio mínimo (R$)</span>
                <Controller
                    name={"minimumBounty"}
                    control={control}
                    render={({ field }) => (
                        <IMaskInput
                            mask={Number}
                            value={String(field.value)}
                            scale={2}
                            thousandsSeparator="."
                            padFractionalZeros={false}
                            normalizeZeros={true}
                            radix={","}
                            mapToRadix={["."]}
                            onComplete={field.onChange}
                        />
                    )}
                />
            </label>
            <label className="company-input col-span-2">
                <span>Comissão base (%)</span>
                <Controller
                    name={"baseCommission"}
                    control={control}
                    render={({ field }) => (
                        <IMaskInput
                            mask={Number}
                            value={String(field.value)}
                            scale={2}
                            thousandsSeparator="."
                            padFractionalZeros={false}
                            normalizeZeros={true}
                            radix={","}
                            mapToRadix={["."]}
                            onComplete={field.onChange}
                        />
                    )}
                />
            </label>
            <label className="company-input col-span-2">
                <span>Comissão máxima (%)</span>
                <Controller
                    name={"maximumCommission"}
                    control={control}
                    render={({ field }) => (
                        <IMaskInput
                            mask={Number}
                            value={String(field.value)}
                            scale={2}
                            thousandsSeparator="."
                            padFractionalZeros={false}
                            normalizeZeros={true}
                            radix={","}
                            mapToRadix={["."]}
                            onComplete={field.onChange}
                        />
                    )}
                />
            </label>
            <label className="company-input col-span-2">
                <span>Prazo médio transferência de corretagem (horas)</span>
                <Controller
                    name={"minimumBrokerage"}
                    control={control}
                    render={({ field }) => (
                        <IMaskInput
                            mask={Number}
                            value={String(field.value)}
                            scale={2}
                            thousandsSeparator="."
                            padFractionalZeros={false}
                            normalizeZeros={true}
                            radix={","}
                            mapToRadix={["."]}
                            onComplete={field.onChange}
                        />
                    )}
                />
            </label>
            <label className="company-input">
                <span>Retroatividade</span>
                <Controller
                    name={"externalRetroactivity"}
                    control={control}
                    render={({ field }) => (
                        <ReactSelect
                            options={retroactivities}
                            getOptionLabel={(option) => option.label}
                            getOptionValue={(option) => option.value}
                            value={retroactivities.find(x => Number(x.value) === field.value)}
                            onChange={x => field.onChange(x?.value)}
                            styles={{
                                control: (baseStyles, state) => ({
                                    ...baseStyles,
                                    borderRadius: '8px',
                                    backgroundColor: 'white',
                                    border: state.isFocused ? "1px solid #a855f7" : "1px solid #d4d4d4",
                                    "&:hover": {
                                        border: state.isFocused ? "1px solid #a855f7" : "1px solid #d4d4d4"
                                    },
                                    height: '45px',
                                    width: '100%',
                                    boxShadow: 'none',
                                }),
                                option: (baseStyles, state) => ({
                                    ...baseStyles,
                                    color: state.isSelected ? 'white' : '#333',
                                    backgroundColor: state.isSelected ? '#a855f7' : 'white',
                                    "&:hover": {
                                        backgroundColor: '#a855f7',
                                        color: '#FFF'
                                    }
                                }),
                                menu: (base, state) => ({
                                    ...base,
                                    marginTop: '4px'
                                }),
                                container: (baseStyles, state) => ({
                                    ...baseStyles,
                                    marginBottom: '5px',
                                    marginTop: '5px'
                                })
                            }}
                            placeholder="Selecione" />
                    )}
                />
            </label>
            <div className="flex items-center mt-5">
                <label className="checkbox-control">
                    <input type={"checkbox"} {...register('internalRetroactivity')}></input>
                    <span>Retroativ. interna</span>
                </label>
            </div>
            <div className="flex items-center mt-5">
                <label className="checkbox-control">
                    <input type={"checkbox"} {...register('exclusive')}></input>
                    <span>Dedicação exclusiva</span>
                </label>
            </div>
            {
                getContractType() === 'PUBLIC_CONTRACT' && (
                    <>
                        <label className="company-input col-span-2">
                            <span>Regra agravo trabalhista</span>
                            <input {...register('grievanceRule')} disabled></input>
                        </label>
                        <div className="flex items-center mt-5 col-span-3">
                            <label className="checkbox-control">
                                <input type={"checkbox"} {...register('pstp')}></input>
                                <span>Aceita prestação de serviço com trabalhista e previdenciário</span>
                            </label>
                        </div>
                    </>
                )
            }
        </div>
        <button type={"submit"} className="company-button-first p-2 w-64 self-end">Salvar</button>
    </form>
}

export default CompanyForm;