CREATE TABLE public.bulk_pricing (
	bulk_pricing_id int4 GENERATED ALWAYS AS IDENTITY( INCREMENT BY 1 MINVALUE 1 MAXVALUE 2147483647 START 1 CACHE 1 NO CYCLE) NOT NULL,
	shopify_product_id varchar NOT NULL,
	shopify_variant_id varchar NOT NULL,
	quantity int4 NULL,
	bulk_price numeric(10, 2) NULL,
	utc_created_at timestamptz NULL,
	utc_updated_at timestamptz NULL,
	CONSTRAINT bulk_pricing_pkey PRIMARY KEY (bulk_pricing_id),
	CONSTRAINT variant_quantity UNIQUE (shopify_variant_id, quantity)
);
CREATE INDEX product_quantity ON public.bulk_pricing USING btree (shopify_product_id, quantity);

CREATE TABLE public.company_pricing (
	company_pricing_id int4 GENERATED ALWAYS AS IDENTITY( INCREMENT BY 1 MINVALUE 1 MAXVALUE 2147483647 START 1 CACHE 1 NO CYCLE) NOT NULL,
	brand_id public."brand_enum" NOT NULL,
	shopify_company_id varchar NOT NULL,
	shopify_product_id varchar NOT NULL,
	shopify_variant_id varchar NOT NULL,
	contract_price numeric(11, 4) NULL,
	last_purchase_price numeric(11, 4) NULL,
	utc_created_at timestamptz NULL,
	utc_updated_at timestamptz NULL,
	CONSTRAINT brand_company_variant UNIQUE (brand_id, shopify_company_id, shopify_variant_id),
	CONSTRAINT company_price_pkey PRIMARY KEY (company_pricing_id)
);
CREATE INDEX brand_company_product ON public.company_pricing USING btree (brand_id, shopify_company_id, shopify_product_id);

CREATE TABLE public.product_map (
	product_map_id int4 GENERATED ALWAYS AS IDENTITY( INCREMENT BY 1 MINVALUE 1 MAXVALUE 2147483647 START 1 CACHE 1 NO CYCLE) NOT NULL,
	shopify_id varchar NULL,
	oracle_id varchar NULL,
	product_hash varchar NULL,
	utc_created_at timestamptz NULL,
	utc_updated_at timestamptz NULL,
	shopify_product_handle varchar DEFAULT ''::character varying NOT NULL,
	CONSTRAINT product_map_pkey PRIMARY KEY (product_map_id)
);
CREATE INDEX product_map_oracle_id ON public.product_map USING btree (oracle_id);

CREATE TABLE public.template_pricing (
	template_pricing_id int4 GENERATED ALWAYS AS IDENTITY( INCREMENT BY 1 MINVALUE 1 MAXVALUE 2147483647 START 1 CACHE 1 NO CYCLE) NOT NULL,
	template_name varchar NOT NULL,
	shopify_product_id varchar NOT NULL,
	shopify_variant_id varchar NOT NULL,
	template_price numeric(11, 4) NULL,
	utc_created_at timestamptz NULL,
	utc_updated_at timestamptz NULL,
	CONSTRAINT name_variant UNIQUE (template_name, shopify_variant_id),
	CONSTRAINT template_pricing_pkey PRIMARY KEY (template_pricing_id)
);
CREATE INDEX template_product ON public.template_pricing USING btree (template_name, shopify_product_id);

CREATE TABLE public.variant_map (
	variant_map_id int4 GENERATED ALWAYS AS IDENTITY( INCREMENT BY 1 MINVALUE 1 MAXVALUE 2147483647 START 1 CACHE 1 NO CYCLE) NOT NULL,
	shopify_product_id varchar NULL,
	shopify_variant_id varchar NULL,
	oracle_id varchar NULL,
	utc_created_at timestamptz NULL,
	utc_updated_at timestamptz NULL,
	CONSTRAINT product_variant UNIQUE (shopify_product_id, shopify_variant_id),
	CONSTRAINT variant_map_pkey PRIMARY KEY (variant_map_id)
);
CREATE INDEX variant_map_oracle_id ON public.variant_map USING btree (oracle_id);

CREATE USER cql PASSWORD '<put password here>'
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO cql
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO cql
